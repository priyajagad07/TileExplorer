using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [Header("UI Elements")]
    public CanvasGroup tutorialOverlay;
    public RectTransform pointer;

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

    // --- THE ULTIMATE SIMPLIFIED FOCUS TRICK ---

    private void SetUIFocus(GameObject target, bool isFocused, int sortOrder = 30000)
    {
        if (target == null) return;

        Canvas canvas = target.GetComponent<Canvas>();
        
        if (isFocused)
        {
            // Add components only if they are missing
            if (canvas == null) canvas = target.AddComponent<Canvas>();
            if (target.GetComponent<GraphicRaycaster>() == null) target.AddComponent<GraphicRaycaster>();

            // Pop it out of the darkness!
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortOrder;
        }
        else
        {
            // Put it back to normal instantly by turning off the override
            if (canvas != null) canvas.overrideSorting = false;
        }
    }

    // --- STRICT TILE TUTORIAL (LEVEL 1) ---

    public void CheckAndStartTutorial()
    {
        if (PlayerPrefs.GetInt("Level", 0) == 0 && PlayerPrefs.GetInt("TutorialCompleted", 0) == 0)
        {
            isTutorialActive = true;
            tutorialStep = 0;

            tutorialOverlay.gameObject.SetActive(true);
            tutorialOverlay.alpha = 0f;
            pointer.gameObject.SetActive(false);

            tutorialOverlay.DOFade(0.7f, 0.5f);
            DOVirtual.DelayedCall(0.5f, () => ShowNextStep());
        }
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

        // Pop the tile to 30000, and the Pointer to 30001 so the hand stays on top!
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
            
            pointer.DOKill();
            pointer.gameObject.SetActive(false);
            SetUIFocus(pointer.gameObject, false); // Put pointer back to normal

            if (currentTargetTile != null)
            {
                currentTargetTile.transform.DOKill();
                currentTargetTile.transform.localScale = Vector3.one;
                SetUIFocus(currentTargetTile, false); // Put tile back to normal
            }

            DOVirtual.DelayedCall(0.4f, () => ShowNextStep());
            return true;
        }
        return false;
    }

    private void EndTutorial()
    {
        isTutorialActive = false;
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();

        pointer.DOKill();
        pointer.gameObject.SetActive(false);
        SetUIFocus(pointer.gameObject, false);

        tutorialOverlay.DOFade(0f, 0.5f).OnComplete(() =>
        {
            tutorialOverlay.gameObject.SetActive(false);
        });

        if (currentTargetTile != null)
        {
            currentTargetTile.transform.DOKill();
            currentTargetTile.transform.localScale = Vector3.one;
            SetUIFocus(currentTargetTile, false);
        }
    }

    // --- SOFT BOOSTER TUTORIAL ---

    public void StartBoosterTutorial(RectTransform boosterRect, string boosterName)
    {
        string prefKey = boosterName + "_TutorialSeen";

        if (PlayerPrefs.GetInt(prefKey, 0) == 1) return;

        isSoftTutorialActive = true;
        currentSoftTutorialKey = prefKey;
        activeBooster = boosterRect.gameObject; 

        tutorialOverlay.gameObject.SetActive(true);
        tutorialOverlay.alpha = 0f;
        tutorialOverlay.DOKill();
        tutorialOverlay.DOFade(0.7f, 0.5f);

        pointer.DOKill();
        pointer.gameObject.SetActive(true);

        pointer.position = boosterRect.position;
        pointer.anchoredPosition += new Vector2(60f, -60f); 
        pointer.SetAsLastSibling();

        pointer.localScale = Vector3.one;
        pointer.DOScale(1.2f, 0.45f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutQuad);

        // Pop the Booster to 30000, and the Pointer to 30001!
        SetUIFocus(activeBooster, true, 30000);
        SetUIFocus(pointer.gameObject, true, 30001);
    }

    public void CloseSoftTutorial()
    {
        if (!isSoftTutorialActive) return;

        isSoftTutorialActive = false;
        
        PlayerPrefs.SetInt(currentSoftTutorialKey, 1);
        PlayerPrefs.Save();

        pointer.DOKill();
        pointer.gameObject.SetActive(false);
        SetUIFocus(pointer.gameObject, false); // Put pointer back to normal

        tutorialOverlay.DOKill();
        tutorialOverlay.DOFade(0f, 0.4f).OnComplete(() => 
        {
            tutorialOverlay.gameObject.SetActive(false);
        });

        if (activeBooster != null)
        {
            SetUIFocus(activeBooster, false); // Put booster back to normal
            activeBooster = null;
        }
    }
}