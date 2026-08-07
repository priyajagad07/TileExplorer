using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Soft Undo booster tutorial (Level 3).
/// Shares the same overlay/popup/pointer UI as TutorialManager when wired
/// to the same Inspector references.
/// </summary>
public class UndoTutorialManager : MonoBehaviour
{
    public static UndoTutorialManager instance;

    [Header("Tutorial UI (wire same refs as TutorialManager)")]
    [SerializeField] private CanvasGroup tutorialOverlay;
    [SerializeField] private RectTransform pointer;
    [SerializeField] private CanvasGroup tutorialPopupImage;
    [SerializeField] private TMP_Text tutorialText;

    private enum UndoStage
    {
        None,
        TapWrongTile,
        TapUndo
    }

    private UndoStage stage = UndoStage.None;

    private GameObject targetTile;
    private RectTransform undoButton;

    private Vector3 targetOriginalScale;
    private Vector3 undoButtonOriginalScale;

    private Tween targetPulseTween;
    private Tween undoPulseTween;
    private Tween stepTransitionTween;

    // Same sorting orders as TutorialManager / Shuffle soft tutorial.
    private const int TargetFocusOrder = 30000;
    private const int PointerFocusOrder = 30001;
    private const int PopupFocusOrder = 30005;

    // Same hand offsets as working tutorials.
    private static readonly Vector2 TilePointerOffset = new Vector2(70f, -70f);
    private static readonly Vector2 BoosterPointerOffset = new Vector2(60f, -60f);

    public bool IsRunning => stage != UndoStage.None;

    public bool IsWaitingForUndoTap => stage == UndoStage.TapUndo;

    private class FocusState
    {
        public Canvas canvas;
        public bool canvasWasAdded;
        public bool previousOverrideSorting;
        public int previousSortingOrder;
        public GraphicRaycaster raycaster;
        public bool raycasterWasAdded;
    }

    private readonly Dictionary<GameObject, FocusState> focusedObjects =
        new Dictionary<GameObject, FocusState>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnDisable()
    {
        stepTransitionTween?.Kill();
        stepTransitionTween = null;

        targetPulseTween?.Kill();
        targetPulseTween = null;

        undoPulseTween?.Kill();
        undoPulseTween = null;

        if (IsRunning)
        {
            TutorialAnalyticsTracker.Instance
                ?.CancelCurrentTutorial();
        }

        ClearAllUIFocus();
    }

    // ==========================================
    // START
    // ==========================================

    public void StartTutorial(RectTransform undoButtonRect)
    {
        if (IsRunning)
            return;

        if (SaveManager.instance == null ||
            SaveManager.instance.data == null)
        {
            return;
        }

        if (SaveManager.instance.data.softTutorialsSeen.Contains("Undo"))
            return;

        if (undoButtonRect == null)
        {
            Debug.LogWarning("Undo Tutorial: Undo button is missing.");
            return;
        }

        undoButton = undoButtonRect;
        stage = UndoStage.TapWrongTile;

        if (IdleHintManager.instance != null)
        {
            IdleHintManager.instance.StopHints();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetTutorialBannerSuppressed(true);
        }

        TutorialAnalyticsTracker.Instance
            ?.BeginTutorial("Undo_booster_tutorial");

        SetupOverlay();

        targetTile = FindUndoTutorialTile();

        if (targetTile == null)
        {
            Debug.LogWarning("Undo Tutorial: no available tile.");
            CancelTutorial();
            return;
        }

        ShowWrongTileStep();
    }

    private void SetupOverlay()
    {
        if (tutorialOverlay != null)
        {
            tutorialOverlay.gameObject.SetActive(true);
            tutorialOverlay.blocksRaycasts = false;

            Image image = tutorialOverlay.GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = false;
            }

            tutorialOverlay.DOKill();
            tutorialOverlay.alpha = 0f;
            tutorialOverlay.DOFade(0.7f, 0.4f).SetUpdate(true);
        }

        if (tutorialPopupImage != null)
        {
            tutorialPopupImage.gameObject.SetActive(true);
            tutorialPopupImage.alpha = 1f;
            SetUIFocus(tutorialPopupImage.gameObject, true, PopupFocusOrder);
        }

        /*
         * Do NOT focus the pointer here.
         * Focus it only when placing it, AFTER the target
         * (same order as Level 1 / Shuffle). Focusing it
         * before the tile let the tile Canvas render on top.
         *
         * Across step 1 → 2 we still keep pointer focus alive
         * by never calling SetUIFocus(pointer, false) in between.
         */
    }

    // ==========================================
    // STEP 1 — tap a tile (soft)
    // ==========================================

    private void ShowWrongTileStep()
    {
        if (targetTile == null)
        {
            CancelTutorial();
            return;
        }

        if (tutorialText != null)
        {
            tutorialText.text = "Oops! Tap this tile by mistake.";
        }

        if (tutorialPopupImage != null)
        {
            tutorialPopupImage.transform.DOKill();
            tutorialPopupImage.transform.localScale = Vector3.zero;
            tutorialPopupImage.transform
                .DOScale(Vector3.one, 0.45f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        // Exact same order as Level 1 TutorialManager.ShowNextStep():
        // place hand → focus tile → focus pointer LAST so hand stays above tile.
        PlacePointerOnTarget(
            targetTile.GetComponent<RectTransform>(),
            TilePointerOffset
        );

        SetUIFocus(targetTile, true, TargetFocusOrder);
        SetUIFocus(pointer.gameObject, true, PointerFocusOrder);

        // Newly added tile Canvas can win a frame of sorting over an
        // earlier pointer Canvas; force a rebuild after pointer is last.
        Canvas.ForceUpdateCanvases();

        targetTile.transform.DOKill();
        targetOriginalScale = targetTile.transform.localScale;

        targetPulseTween?.Kill();
        targetPulseTween = targetTile.transform
            .DOScale(targetOriginalScale * 1.15f, 0.4f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true);

        Debug.Log("Undo Tutorial: Tap wrong tile.");
    }

    /// <summary>
    /// Soft tutorial gate from Tile.OnPointerClick.
    /// Target tile continues; any other tile dismisses free Undo use.
    /// Always returns true so normal gameplay is not blocked.
    /// </summary>
    public bool HandleTileClick(GameObject clickedTile)
    {
        if (!IsRunning)
            return true;

        if (stage == UndoStage.TapWrongTile)
        {
            if (clickedTile == targetTile)
                return true;

            // Player ignored the soft tutorial — free Undo is lost,
            // but saved inventory is untouched.
            CancelTutorial();
            return true;
        }

        if (stage == UndoStage.TapUndo)
        {
            // Tapped a tile instead of Undo — dismiss soft tutorial.
            CancelTutorial();
            return true;
        }

        return true;
    }

    /// <summary>
    /// Call BEFORE tray animation starts.
    /// Stops tile pulse / focus without killing the move tween.
    /// Keeps pointer Canvas focus alive for step 2.
    /// </summary>
    public void PrepareTargetTileMove(GameObject tile)
    {
        if (!IsRunning || stage != UndoStage.TapWrongTile)
            return;

        if (tile != targetTile)
            return;

        targetPulseTween?.Kill();
        targetPulseTween = null;

        if (targetTile != null)
        {
            targetTile.transform.localScale = targetOriginalScale;
            SetUIFocus(targetTile, false);
        }

        // Hide pointer visually only — do NOT remove UI focus.
        if (pointer != null)
        {
            pointer.DOKill();
            pointer.gameObject.SetActive(false);
        }
    }

    // ==========================================
    // STEP 2 — tap Undo (soft, free use)
    // ==========================================

    public void OnTileMovedToTray(GameObject tile)
    {
        if (!IsRunning || stage != UndoStage.TapWrongTile)
            return;

        if (tile != targetTile)
            return;

        stage = UndoStage.TapUndo;

        /*
         * Wait one frame so:
         * 1) tile Canvas/GraphicRaycaster cleanup can finish
         * 2) we never tear down / rebuild pointer focus in the
         *    same frame as focusing the Undo button
         *
         * Then match tray arrival (~0.15s) before showing step 2.
         */
        stepTransitionTween?.Kill();
        stepTransitionTween = DOVirtual.DelayedCall(
            0.18f,
            () =>
            {
                stepTransitionTween = null;

                if (stage == UndoStage.TapUndo)
                {
                    ShowUndoStep();
                }
            }
        ).SetUpdate(true);
    }

    private void ShowUndoStep()
    {
        if (undoButton == null)
        {
            CancelTutorial();
            return;
        }

        if (tutorialText != null)
        {
            tutorialText.text = "Oops! Tap Undo to fix your mistake.";
        }

        if (tutorialPopupImage != null)
        {
            tutorialPopupImage.transform.DOKill();
            tutorialPopupImage.transform
                .DOPunchScale(Vector3.one * 0.12f, 0.3f, 5, 0.5f)
                .SetUpdate(true);
        }

        // Exact same placement style as Shuffle StartBoosterTutorial().
        PlacePointerOnTarget(undoButton, BoosterPointerOffset);

        // Same order as Shuffle: booster first, pointer LAST (above target).
        SetUIFocus(undoButton.gameObject, true, TargetFocusOrder);
        SetUIFocus(pointer.gameObject, true, PointerFocusOrder);

        undoButtonOriginalScale = undoButton.localScale;

        undoPulseTween?.Kill();
        undoPulseTween = undoButton
            .DOScale(undoButtonOriginalScale * 1.1f, 0.4f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true);

        Debug.Log("Undo Tutorial: Tap Undo.");
    }

    /// <summary>
    /// Same coordinate approach as Level 1 / Shuffle:
    /// world position of target, then anchored offset.
    /// </summary>
    private void PlacePointerOnTarget(
        RectTransform target,
        Vector2 anchoredOffset)
    {
        if (pointer == null || target == null)
            return;

        pointer.DOKill();
        pointer.gameObject.SetActive(true);

        pointer.position = target.position;
        pointer.anchoredPosition += anchoredOffset;
        pointer.SetAsLastSibling();

        pointer.localScale = Vector3.one;
        pointer
            .DOScale(1.2f, 0.45f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    // ==========================================
    // FINISH
    // ==========================================

    public void CompleteFreeUndo()
    {
        if (!IsWaitingForUndoTap)
            return;

        FinishTutorial();
    }

    public void CancelTutorial()
    {
        if (!IsRunning)
            return;

        FinishTutorial();
    }

    private void FinishTutorial()
    {
        stage = UndoStage.None;

        stepTransitionTween?.Kill();
        stepTransitionTween = null;

        targetPulseTween?.Kill();
        targetPulseTween = null;

        undoPulseTween?.Kill();
        undoPulseTween = null;

        if (targetTile != null)
        {
            Tile tile = targetTile.GetComponent<Tile>();
            if (tile != null && !tile.IsMoved())
            {
                targetTile.transform.localScale = targetOriginalScale;
            }
        }

        if (undoButton != null)
        {
            undoButton.DOKill();
            undoButton.localScale = undoButtonOriginalScale;
        }

        if (pointer != null)
        {
            pointer.DOKill();
            pointer.gameObject.SetActive(false);
        }

        if (SaveManager.instance != null &&
            SaveManager.instance.data != null &&
            !SaveManager.instance.data.softTutorialsSeen.Contains("Undo"))
        {
            SaveManager.instance.data.softTutorialsSeen.Add("Undo");
            SaveManager.instance.SaveData();
        }

        TutorialAnalyticsTracker.Instance
            ?.CompleteTutorial("Undo_booster_tutorial");

        if (tutorialPopupImage != null)
        {
            tutorialPopupImage.transform.DOKill();
            SetUIFocus(tutorialPopupImage.gameObject, false);
            tutorialPopupImage
                .DOFade(0f, 0.3f)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (tutorialPopupImage != null)
                    {
                        tutorialPopupImage.gameObject.SetActive(false);
                    }
                });
        }

        if (undoButton != null)
        {
            SetUIFocus(undoButton.gameObject, false);
        }

        if (pointer != null)
        {
            SetUIFocus(pointer.gameObject, false);
        }

        if (targetTile != null)
        {
            SetUIFocus(targetTile, false);
        }

        if (tutorialOverlay != null)
        {
            tutorialOverlay.DOKill();
            tutorialOverlay
                .DOFade(0f, 0.35f)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    tutorialOverlay.gameObject.SetActive(false);
                    ClearAllUIFocus();

                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance
                            .SetTutorialBannerSuppressed(false);
                    }

                    if (IdleHintManager.instance != null &&
                        !(TutorialManager.instance != null &&
                          TutorialManager.instance.IsAnyTutorialActive))
                    {
                        IdleHintManager.instance.ResetIdleTimer();
                    }
                });
        }
        else
        {
            ClearAllUIFocus();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetTutorialBannerSuppressed(false);
            }

            if (IdleHintManager.instance != null)
            {
                IdleHintManager.instance.ResetIdleTimer();
            }
        }

        targetTile = null;
        undoButton = null;
    }

    // ==========================================
    // FIND TILE
    // ==========================================

    private GameObject FindUndoTutorialTile()
    {
        Tile[] allTiles =
            FindObjectsByType<Tile>(FindObjectsSortMode.None);

        GameObject bestTile = null;
        float bestDistance = float.MinValue;

        Vector3 popupPosition =
            tutorialPopupImage != null
                ? tutorialPopupImage.transform.position
                : Vector3.zero;

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

            float distance =
                (tile.transform.position - popupPosition).sqrMagnitude;

            if (distance > bestDistance)
            {
                bestDistance = distance;
                bestTile = tile.gameObject;
            }
        }

        return bestTile;
    }

    // ==========================================
    // UI FOCUS (same pattern as TutorialManager)
    // ==========================================

    private void SetUIFocus(
        GameObject target,
        bool focused,
        int sortOrder = TargetFocusOrder)
    {
        if (target == null)
            return;

        if (focused)
        {
            if (focusedObjects.TryGetValue(target, out FocusState existing))
            {
                if (existing.canvas != null)
                {
                    existing.canvas.overrideSorting = true;
                    existing.canvas.sortingOrder = sortOrder;
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
                state.previousOverrideSorting = canvas.overrideSorting;
                state.previousSortingOrder = canvas.sortingOrder;
            }

            state.canvas = canvas;

            GraphicRaycaster raycaster =
                target.GetComponent<GraphicRaycaster>();

            if (raycaster == null)
            {
                raycaster = target.AddComponent<GraphicRaycaster>();
                state.raycasterWasAdded = true;
            }

            state.raycaster = raycaster;
            focusedObjects[target] = state;

            canvas.overrideSorting = true;
            canvas.sortingOrder = sortOrder;
            return;
        }

        if (!focusedObjects.TryGetValue(target, out FocusState stateToRestore))
            return;

        if (stateToRestore.canvas != null && !stateToRestore.canvasWasAdded)
        {
            stateToRestore.canvas.overrideSorting =
                stateToRestore.previousOverrideSorting;
            stateToRestore.canvas.sortingOrder =
                stateToRestore.previousSortingOrder;
        }

        if (stateToRestore.canvasWasAdded && stateToRestore.canvas != null)
        {
            stateToRestore.canvas.overrideSorting = false;
            stateToRestore.canvas.sortingOrder = 0;
        }

        if (stateToRestore.raycasterWasAdded &&
            stateToRestore.raycaster != null)
        {
            Destroy(stateToRestore.raycaster);
        }

        if (stateToRestore.canvasWasAdded &&
            stateToRestore.canvas != null &&
            isActiveAndEnabled)
        {
            StartCoroutine(RemoveCanvasNextFrame(stateToRestore.canvas));
        }

        focusedObjects.Remove(target);
    }

    private IEnumerator RemoveCanvasNextFrame(Canvas canvas)
    {
        yield return null;

        if (canvas == null)
            yield break;

        if (canvas.GetComponent<GraphicRaycaster>() != null)
            yield break;

        Destroy(canvas);
    }

    private void ClearAllUIFocus()
    {
        GameObject[] objects = new GameObject[focusedObjects.Count];
        focusedObjects.Keys.CopyTo(objects, 0);

        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                SetUIFocus(obj, false);
            }
        }

        focusedObjects.Clear();
    }
}
