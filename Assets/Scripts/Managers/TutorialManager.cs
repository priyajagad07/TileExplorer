using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

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

    private const int TOTAL_STEPS = 3;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (isSoftTutorialActive && Input.GetMouseButtonDown(0))
        {
            CloseSoftTutorial();
        }
    }

    private void SetUIFocus(GameObject target, bool isFocused, int sortOrder = 30000)
    {
        if (target == null) return;

        Canvas canvas = target.GetComponent<Canvas>();

        if (isFocused)
        {
            if (canvas == null) canvas = target.AddComponent<Canvas>();
            if (target.GetComponent<GraphicRaycaster>() == null) target.AddComponent<GraphicRaycaster>();

            canvas.overrideSorting = true;
            canvas.sortingOrder = sortOrder;
        }
        else
        {
            if (canvas != null) canvas.overrideSorting = false;
        }
    }

    public void CheckAndStartTutorial()
    {
        if (SaveManager.instance.data.level == 0 && SaveManager.instance.data.tutorialCompleted == 0)
        {
            if (topHeaderUI != null) topHeaderUI.SetActive(false);
            if (boosterUI != null) boosterUI.SetActive(false);

            isTutorialActive = true;
            tutorialStep = 0;

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
                SetUIFocus(tutorialPopupImage.gameObject, true, 30005);
            }

            if (MatchBoard.instance != null)
            {
                SetUIFocus(MatchBoard.instance.gameObject, true, 29999);
            }

            tutorialOverlay.DOFade(0.7f, 0.5f);
            DOVirtual.DelayedCall(0.5f, () => ShowNextStep());
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

        tutorialText.text = stepMessages[step].Replace("/n", "\n");

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

        SetUIFocus(currentTargetTile, true, 30000);
        SetUIFocus(pointer.gameObject, true, 30001);

        currentTargetTile.transform.DOKill();
        currentTargetTile.transform.DOScale(1.15f, 0.4f).SetLoops(-1, LoopType.Yoyo);
    }

    private GameObject FindValidTile()
    {
        Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);
        foreach (Tile tile in allTiles)
        {
            if (!MatchBoard.instance.GetPlacedTiles().Contains(tile.gameObject))
            {
                return tile.gameObject;
            }
        }
        return null;
    }

    public bool IsTileClickAllowed(GameObject clickedTile)
    {
        if (!isTutorialActive) return true;

        if (clickedTile == currentTargetTile)
        {
            tutorialStep++;

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
                currentTargetTile.transform.localScale = Vector3.one;
                SetUIFocus(currentTargetTile, false);
            }

            DOVirtual.DelayedCall(0.4f, () => ShowNextStep());
            return true;
        }
        return false;
    }

    private void EndTutorial()
    {
        isTutorialActive = false;
        SaveManager.instance.data.tutorialCompleted = 1;
        SaveManager.instance.SaveData();

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
            currentTargetTile.transform.localScale = Vector3.one;
            SetUIFocus(currentTargetTile, false);
        }

        if (MatchBoard.instance != null)
        {
            SetUIFocus(MatchBoard.instance.gameObject, false);
        }
    }

    public void StartBoosterTutorial(RectTransform boosterRect, string boosterName)
    {
        if (SaveManager.instance.data.softTutorialsSeen.Contains(boosterName)) return;

        isSoftTutorialActive = true;
        currentSoftTutorialKey = boosterName;
        activeBooster = boosterRect.gameObject;

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
                case "Undo": msg = "New Booster Unlocked! \n Tap here to undo a mistake."; break;
                case "Shuffle": msg = "New Booster Unlocked! \n Tap here to shuffle the board."; break;
                case "Magic": msg = "New Booster Unlocked! \n Tap here to auto-match 3 tiles."; break;
                default: msg = "New Booster Unlocked!"; break;
            }

            tutorialText.text = msg;

            // Juicy Pop-in Animation
            tutorialPopupImage.transform.DOKill();
            tutorialPopupImage.transform.localScale = Vector3.zero;
            tutorialPopupImage.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

            SetUIFocus(tutorialPopupImage.gameObject, true, 30005);
        }

        pointer.DOKill();
        pointer.gameObject.SetActive(true);

        pointer.position = boosterRect.position;
        pointer.anchoredPosition += new Vector2(60f, -60f);
        pointer.SetAsLastSibling();

        pointer.localScale = Vector3.one;
        pointer.DOScale(1.2f, 0.45f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutQuad);

        SetUIFocus(activeBooster, true, 30000);
        SetUIFocus(pointer.gameObject, true, 30001);

        activeBooster.transform.DOKill();
        activeBooster.transform.DOScale(1.1f, 0.4f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
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
        tutorialOverlay.DOFade(0f, 0.4f).OnComplete(() =>
        {
            tutorialOverlay.gameObject.SetActive(false);
        });

        if (activeBooster != null)
        {
            activeBooster.transform.DOKill();
            activeBooster.transform.localScale = Vector3.one;
            SetUIFocus(activeBooster, false);
            activeBooster = null;
        }

        if (MatchBoard.instance != null)
        {
            MatchBoard.instance.isInputLocked = false;
        }
    }
}