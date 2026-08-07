using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [Header("UI Elements")]
    public CanvasGroup tutorialOverlay;
    public RectTransform pointer;
    public CanvasGroup tutorialPopupImage;
    public TMP_Text tutorialText;

    [Header("UI To Hide During Tutorial")]
    public GameObject topHeaderUI;
    public GameObject boosterUI;

    [Header("Tutorial Messages")]
    [TextArea]
    public string[] stepMessages = new string[] {
        "Tap the highlighted tile to move it to your tray!",
        "Great! \n Now tap another matching tile.",
        "One more! \n Match 3 identical tiles to clear them!"
    };

    [Header("State")]
    public bool isTutorialActive = false;
    public bool isSoftTutorialActive = false;

    private GameObject currentTargetTile;
    private GameObject activeBooster;
    private int tutorialStep = 0;
    private string currentSoftTutorialKey = "";
    private Vector3 currentTargetOriginalScale;

    private const int TOTAL_STEPS = 3;
    private Vector3 activeBoosterOriginalScale;
    private Tween tutorialDelayedCall;

    private const string MainTutorialAnalyticsName =
    "main_gameplay_tutorial";

    private string currentSoftTutorialAnalyticsName =
        string.Empty;

    private const int MatchBoardFocusOrder = 29999;
    private const int TargetFocusOrder = 30000;
    private const int PointerFocusOrder = 30001;
    private const int PopupFocusOrder = 30005;

    public bool IsAnyTutorialActive => isTutorialActive || isSoftTutorialActive;

    private class FocusState
    {
        public Canvas canvas;
        public bool canvasWasAdded;
        public bool previousOverrideSorting;
        public int previousSortingOrder;

        public GraphicRaycaster raycaster;
        public bool raycasterWasAdded;
    }

    private readonly System.Collections.Generic.Dictionary<GameObject, FocusState>
        focusedObjects =
            new System.Collections.Generic.Dictionary<GameObject, FocusState>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        if (isTutorialActive ||
            isSoftTutorialActive)
        {
            TutorialAnalyticsTracker.Instance
                ?.CancelCurrentTutorial();
        }

        tutorialDelayedCall?.Kill();
        tutorialDelayedCall = null;

        ClearAllUIFocus();

        if (UIManager.Instance != null)
        {
            UIManager.Instance
                .SetTutorialBannerSuppressed(false);
        }
    }

    private void SetUIFocus(
    GameObject target,
    bool isFocused,
    int sortOrder = TargetFocusOrder)
    {
        if (target == null)
            return;

        if (isFocused)
        {
            // The object is already being focused.
            if (focusedObjects.TryGetValue(
                    target,
                    out FocusState existingState))
            {
                if (existingState.canvas != null)
                {
                    existingState.canvas.overrideSorting = true;
                    existingState.canvas.sortingOrder = sortOrder;
                }

                return;
            }

            FocusState state = new FocusState();

            Canvas canvas = target.GetComponent<Canvas>();

            if (canvas == null)
            {
                canvas = target.AddComponent<Canvas>();
                state.canvasWasAdded = true;
            }
            else
            {
                state.previousOverrideSorting =
                    canvas.overrideSorting;

                state.previousSortingOrder =
                    canvas.sortingOrder;
            }

            state.canvas = canvas;

            GraphicRaycaster raycaster =
                target.GetComponent<GraphicRaycaster>();

            if (raycaster == null)
            {
                raycaster =
                    target.AddComponent<GraphicRaycaster>();

                state.raycasterWasAdded = true;
            }

            state.raycaster = raycaster;

            focusedObjects[target] = state;

            canvas.overrideSorting = true;
            canvas.sortingOrder = sortOrder;
        }
        else
        {
            if (!focusedObjects.TryGetValue(
                    target,
                    out FocusState state))
            {
                Canvas existingCanvas =
     target.GetComponent<Canvas>();

                if (existingCanvas != null)
                {
                    existingCanvas.overrideSorting = false;
                    existingCanvas.sortingOrder = 0;
                }

                return;
            }

            // Restore an existing Canvas immediately.
            if (state.canvas != null &&
                !state.canvasWasAdded)
            {
                state.canvas.overrideSorting =
                    state.previousOverrideSorting;

                state.canvas.sortingOrder =
                    state.previousSortingOrder;
            }

            // Disable sorting immediately so the object stops rendering
            // above the rest of the gameplay UI.
            if (state.canvasWasAdded &&
                state.canvas != null)
            {
                state.canvas.overrideSorting = false;
                state.canvas.sortingOrder = 0;
            }

            // GraphicRaycaster depends on Canvas, so remove it first.
            if (state.raycasterWasAdded &&
                state.raycaster != null)
            {
                Destroy(state.raycaster);
            }

            // Canvas must be removed on a later frame, after Unity has
            // finished removing the GraphicRaycaster.
            if (state.canvasWasAdded &&
                state.canvas != null)
            {
                if (isActiveAndEnabled)
                {
                    StartCoroutine(
                        RemoveAddedCanvasNextFrame(state.canvas)
                    );
                }
            }

            focusedObjects.Remove(target);
        }
    }

    private IEnumerator RemoveAddedCanvasNextFrame(
    Canvas canvas)
    {
        // Destroy() removes the GraphicRaycaster at the end
        // of the current frame.
        yield return null;

        if (canvas == null)
            yield break;

        // Safety check: never remove a Canvas while another
        // GraphicRaycaster still depends on it.
        GraphicRaycaster remainingRaycaster =
            canvas.GetComponent<GraphicRaycaster>();

        if (remainingRaycaster != null)
        {
            Debug.LogWarning(
                $"Tutorial cleanup kept Canvas on " +
                $"{canvas.gameObject.name} because a " +
                $"GraphicRaycaster is still attached."
            );

            canvas.overrideSorting = false;
            canvas.sortingOrder = 0;
            yield break;
        }

        Destroy(canvas);
    }

    public void CheckAndStartTutorial()
    {
        if (SaveManager.instance.data.level == 0 && SaveManager.instance.data.tutorialCompleted == 0)
        {
            if (IdleHintManager.instance != null)
            {
                IdleHintManager.instance.StopHints();
            }

            if (topHeaderUI != null) topHeaderUI.SetActive(false);
            if (boosterUI != null) boosterUI.SetActive(false);

            isTutorialActive = true;
            tutorialStep = 0;

            TutorialAnalyticsTracker.Instance?.BeginTutorial(
        MainTutorialAnalyticsName
    );

            tutorialOverlay.gameObject.SetActive(true);

            tutorialOverlay.blocksRaycasts = true;
            Image overlayImg = tutorialOverlay.GetComponent<Image>();
            if (overlayImg != null) overlayImg.raycastTarget = true;

            tutorialOverlay.alpha = 0f;
            pointer.gameObject.SetActive(false);

            if (tutorialPopupImage != null)
            {
                tutorialPopupImage.gameObject.SetActive(true);
                tutorialPopupImage.alpha = 1f;
                UpdateTutorialText(0);
                SetUIFocus(tutorialPopupImage.gameObject, true, PopupFocusOrder);
            }

            if (MatchBoard.instance != null)
            {
                SetUIFocus(MatchBoard.instance.gameObject, true, MatchBoardFocusOrder);
            }

            tutorialOverlay.DOFade(0.7f, 0.5f);

            tutorialDelayedCall?.Kill();
            tutorialDelayedCall = DOVirtual.DelayedCall(0.5f, () =>
            {
                tutorialDelayedCall = null;

                if (isTutorialActive)
                {
                    ShowNextStep();
                }
            }
    )
    .SetUpdate(true);
        }
        else
        {
            if (topHeaderUI != null) topHeaderUI.SetActive(true);
            if (boosterUI != null) boosterUI.SetActive(true);
            if (tutorialPopupImage != null) tutorialPopupImage.gameObject.SetActive(false);
        }
    }

    private void UpdateTutorialText(int step)
    {
        if (tutorialText == null || tutorialPopupImage == null || step >= stepMessages.Length) return;

        //tutorialText.text = stepMessages[step].Replace("/n", "\n");
        tutorialText.text = stepMessages[step];
        tutorialPopupImage.transform.DOKill();
        tutorialPopupImage.transform.localScale = Vector3.one;
        tutorialPopupImage.transform.DOPunchScale(Vector3.one * 0.15f, 0.35f, 5, 0.5f);
    }

    public void ShowNextStep()
    {
        if (tutorialStep >= TOTAL_STEPS)
        {
            EndTutorial();
            return;
        }

        currentTargetTile = FindValidTile();

        if (currentTargetTile == null)
        {
            EndTutorial();
            return;
        }

        pointer.DOKill();
        pointer.gameObject.SetActive(true);

        RectTransform tileRect = currentTargetTile.GetComponent<RectTransform>();

        if (tileRect != null)
        {
            pointer.position = tileRect.position;
            pointer.anchoredPosition += new Vector2(70f, -70f);
        }

        pointer.SetAsLastSibling();
        pointer.localScale = Vector3.one;
        pointer.DOScale(1.2f, 0.45f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutQuad);

        SetUIFocus(currentTargetTile, true, TargetFocusOrder);
        SetUIFocus(pointer.gameObject, true, PointerFocusOrder);

        currentTargetTile.transform.DOKill();

        currentTargetOriginalScale =
            currentTargetTile.transform.localScale;

        currentTargetTile.transform
            .DOScale(
                currentTargetOriginalScale * 1.15f,
                0.4f
            )
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        Debug.Log("ShowNextStep : " + tutorialStep);
    }

    private GameObject FindValidTile()
    {
        Tile[] allTiles =
            FindObjectsByType<Tile>(
                FindObjectsSortMode.None
            );

        foreach (Tile tile in allTiles)
        {
            if (tile == null)
                continue;

            if (!tile.gameObject.activeInHierarchy)
                continue;

            if (tile.IsMoved())
                continue;

            if (tile.isJellyLocked)
                continue;

            if (tile.IsBlocked())
                continue;

            return tile.gameObject;
        }

        return null;
    }

    public bool IsTileClickAllowed(GameObject clickedTile)
    {
        // -------------------------
        // NORMAL GAMEPLAY
        // -------------------------


        if (!isTutorialActive) return true;

        if (clickedTile == currentTargetTile)
        {
            // Debug.Log("Tutorial Step Before = " + tutorialStep);

            tutorialStep++;

            //Debug.Log("Tutorial Step After = " + tutorialStep);

            if (tutorialStep < stepMessages.Length)
            {
                UpdateTutorialText(tutorialStep);
            }

            pointer.DOKill();
            pointer.gameObject.SetActive(false);
            SetUIFocus(pointer.gameObject, false);

            if (currentTargetTile != null)
            {
                currentTargetTile.transform.DOKill();
                currentTargetTile.transform.localScale = currentTargetOriginalScale;
                SetUIFocus(currentTargetTile, false);
            }

            tutorialDelayedCall?.Kill();

            tutorialDelayedCall =
                DOVirtual.DelayedCall(
                    0.4f,
                    () =>
                    {
                        tutorialDelayedCall = null;

                        if (isTutorialActive)
                        {
                            ShowNextStep();
                        }
                    }
                )
                .SetUpdate(true);
            return true;
        }
        return false;
    }

    private void EndTutorial()
    {
        tutorialDelayedCall?.Kill();
        tutorialDelayedCall = null;
        isTutorialActive = false;
        SaveManager.instance.data.tutorialCompleted = 1;
        SaveManager.instance.SaveData();

        TutorialAnalyticsTracker.Instance
    ?.CompleteTutorial(
        MainTutorialAnalyticsName
    );

        pointer.DOKill();
        pointer.gameObject.SetActive(false);
        SetUIFocus(pointer.gameObject, false);

        tutorialOverlay.blocksRaycasts = false;
        Image overlayImg = tutorialOverlay.GetComponent<Image>();
        if (overlayImg != null) overlayImg.raycastTarget = false;

        if (tutorialPopupImage != null)
        {
            SetUIFocus(tutorialPopupImage.gameObject, false);
            tutorialPopupImage.DOFade(0f, 0.5f).OnComplete(() => tutorialPopupImage.gameObject.SetActive(false));
        }

        tutorialOverlay.DOFade(0f, 0.5f).OnComplete(() =>
        {
            tutorialOverlay.gameObject.SetActive(false);

            if (topHeaderUI != null) topHeaderUI.SetActive(true);
            if (boosterUI != null) boosterUI.SetActive(true);
        });

        if (currentTargetTile != null)
        {
            currentTargetTile.transform.DOKill();
            currentTargetTile.transform.localScale = currentTargetOriginalScale;
            SetUIFocus(currentTargetTile, false);
        }

        if (MatchBoard.instance != null)
        {
            SetUIFocus(MatchBoard.instance.gameObject, false);
        }

        Debug.Log("Tutorial Ended");
        ClearAllUIFocus();
    }

    public void StartBoosterTutorial(RectTransform boosterRect, string boosterName)
    {
        if (SaveManager.instance.data.softTutorialsSeen.Contains(boosterName))
        {
            return;
        }

        if (IdleHintManager.instance != null)
        {
            IdleHintManager.instance.StopHints();
        }

        currentSoftTutorialAnalyticsName =
    boosterName +
    "_booster_tutorial";

        TutorialAnalyticsTracker.Instance
            ?.BeginTutorial(
                currentSoftTutorialAnalyticsName
            );

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetTutorialBannerSuppressed(true);
        }

        isSoftTutorialActive = true;
        currentSoftTutorialKey = boosterName;
        activeBooster = boosterRect.gameObject;

        activeBoosterOriginalScale = activeBooster.transform.localScale;

        tutorialOverlay.gameObject.SetActive(true);

        tutorialOverlay.blocksRaycasts = false;
        Image overlayImg = tutorialOverlay.GetComponent<Image>();
        if (overlayImg != null) overlayImg.raycastTarget = false;

        tutorialOverlay.alpha = 0f;
        tutorialOverlay.DOKill();
        tutorialOverlay.DOFade(0.7f, 0.5f);

        if (tutorialPopupImage != null && tutorialText != null)
        {
            tutorialPopupImage.gameObject.SetActive(true);
            tutorialPopupImage.alpha = 1f;

            string msg = "";
            switch (boosterName)
            {
                //case "Undo": msg = "New Booster Unlocked! \n Tap here to undo a mistake."; break;
                case "Shuffle": msg = "New Booster Unlocked! \n Tap here to shuffle the board."; break;
                case "Magic": msg = "New Booster Unlocked! \n Tap here to auto-match 3 tiles."; break;
                default: msg = "New Booster Unlocked!"; break;
            }

            tutorialText.text = msg;

            // Juicy Pop-in Animation
            tutorialPopupImage.transform.DOKill();
            tutorialPopupImage.transform.localScale = Vector3.zero;
            tutorialPopupImage.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

            SetUIFocus(tutorialPopupImage.gameObject, true, PopupFocusOrder);
        }

        pointer.DOKill();
        pointer.gameObject.SetActive(true);

        pointer.position = boosterRect.position;
        pointer.anchoredPosition += new Vector2(60f, -60f);
        pointer.SetAsLastSibling();

        pointer.localScale = Vector3.one;
        pointer.DOScale(1.2f, 0.45f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutQuad);

        SetUIFocus(activeBooster, true, TargetFocusOrder);
        SetUIFocus(pointer.gameObject, true, PointerFocusOrder);

        activeBooster.transform.DOKill();
        activeBooster.transform.DOScale(activeBoosterOriginalScale * 1.1f, 0.4f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    public void CloseSoftTutorial()
    {
        if (!isSoftTutorialActive) return;

        isSoftTutorialActive = false;

        tutorialOverlay.blocksRaycasts = false;
        Image overlayImg = tutorialOverlay.GetComponent<Image>();
        if (overlayImg != null) overlayImg.raycastTarget = false;

        if (!SaveManager.instance.data.softTutorialsSeen.Contains(currentSoftTutorialKey))
        {
            SaveManager.instance.data.softTutorialsSeen.Add(currentSoftTutorialKey);
            SaveManager.instance.SaveData();
        }

        if (!string.IsNullOrEmpty(
        currentSoftTutorialAnalyticsName))
        {
            TutorialAnalyticsTracker.Instance
                ?.CompleteTutorial(
                    currentSoftTutorialAnalyticsName
                );
        }

        currentSoftTutorialAnalyticsName =
            string.Empty;
        currentSoftTutorialKey =
    string.Empty;

        pointer.DOKill();
        pointer.gameObject.SetActive(false);
        SetUIFocus(pointer.gameObject, false);

        if (tutorialPopupImage != null)
        {
            SetUIFocus(tutorialPopupImage.gameObject, false);
            tutorialPopupImage.transform.DOKill();
            tutorialPopupImage.DOFade(0f, 0.4f).OnComplete(() => tutorialPopupImage.gameObject.SetActive(false));
        }

        tutorialOverlay.DOKill();

        tutorialOverlay
     .DOFade(0f, 0.4f)
     .SetUpdate(true)
     .OnComplete(() =>
     {
         tutorialOverlay.gameObject
             .SetActive(false);

         ClearAllUIFocus();

         if (UIManager.Instance != null)
         {
             UIManager.Instance
                 .SetTutorialBannerSuppressed(false);
         }

         if (IdleHintManager.instance != null &&
             !IsAnyTutorialActive)
         {
             IdleHintManager.instance
                 .ResetIdleTimer();
         }
     });
       
        currentTargetTile = null;

        if (activeBooster != null)
        {
            activeBooster.transform.DOKill();
            activeBooster.transform.localScale = activeBoosterOriginalScale;
            SetUIFocus(activeBooster, false);
            activeBooster = null;
        }

        if (MatchBoard.instance != null)
        {
            MatchBoard.instance.isInputLocked = false;
        }
    }

    private void ClearAllUIFocus()
    {
        GameObject[] targets =
            new GameObject[focusedObjects.Count];

        focusedObjects.Keys.CopyTo(
            targets,
            0
        );

        foreach (GameObject target in targets)
        {
            if (target != null)
            {
                SetUIFocus(target, false);
            }
        }

        focusedObjects.Clear();
    }

    public bool IsBoosterTutorialRunning(
    string boosterName)
    {
        return
            isSoftTutorialActive &&
            currentSoftTutorialKey ==
                boosterName;
    }

    private void PlacePointerAtCenter(RectTransform target)
    {
        if (pointer == null || target == null)
            return;

        // Exact visual center of the target rect in world space
        Vector3 worldCenter =
            target.TransformPoint(target.rect.center);

        pointer.position = worldCenter;
        pointer.SetAsLastSibling();
    }
}