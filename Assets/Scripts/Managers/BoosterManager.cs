using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Solo.MOST_IN_ONE;

public class BoosterManager : MonoBehaviour
{
    public static BoosterManager instance;

    public int undoCount;
    public int shuffleCount;
    public int magicCount;

    [Header("UI Texts")]
    [SerializeField] private List<TextMeshProUGUI> undoText;
    [SerializeField] private List<TextMeshProUGUI> shuffleText;
    [SerializeField] private List<TextMeshProUGUI> magicText;

    [Header("Lock Images")]
    [SerializeField] private GameObject undoLockImage;
    [SerializeField] private GameObject shuffleLockImage;
    [SerializeField] private GameObject magicLockImage;

    [Header("Booster Button Rects (For Animation)")]
    [SerializeField] private RectTransform undoButtonRect;
    [SerializeField] private RectTransform shuffleButtonRect;
    [SerializeField] private RectTransform magicButtonRect;

    [Header("Messages")]
    [SerializeField] private GameObject insuffientCoinUndo;
    [SerializeField] private GameObject insuffientCoinsShuffle;
    [SerializeField] private GameObject insuffientCoinsMagic;
    [SerializeField] private GameObject nothingToUndoMessage;
    [SerializeField] private GameObject nothingToShuffleMessage;
    [SerializeField] private GameObject nothingToMagicMessage;
    [SerializeField] private GameObject lockedBoosterMessage;
    [SerializeField] private TextMeshProUGUI lockedBoosterText;

    [SerializeField] private GameObject cannotUndoShuffleMessage;

    public void ShowCannotUndoShuffle()
    {
        ShowMessage(cannotUndoShuffleMessage);
    }

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

    void Start()
    {
        LoadBoosters();
        CheckUnlockRewards();
        UpdateUI();
    }

    void LoadBoosters()
    {
        undoCount = SaveManager.instance.data.undoCount;
        shuffleCount = SaveManager.instance.data.shuffleCount;
        magicCount = SaveManager.instance.data.magicCount;
    }

    void SaveBoosters()
    {
        GameData data = SaveManager.instance.data;
        data.undoCount = undoCount;
        data.shuffleCount = shuffleCount;
        data.magicCount = magicCount;
        SaveManager.instance.SaveData();
    }

    bool CanEarnUndo()
    {
        return SaveManager.instance != null &&
               SaveManager.instance.data != null &&
               SaveManager.instance.data.undoUnlocked == 1;
    }

    bool CanEarnShuffle()
    {
        return SaveManager.instance != null &&
               SaveManager.instance.data != null &&
               SaveManager.instance.data.shuffleUnlocked == 1;
    }

    bool CanEarnMagic()
    {
        return SaveManager.instance != null &&
               SaveManager.instance.data != null &&
               SaveManager.instance.data.magicUnlocked == 1;
    }

    public void UpdateUI()
    {
        RefreshUndoUI();
        RefreshShuffleUI();
        RefreshMagicUI();
    }

    public void RefreshUndoUI()
    {
        int currentLevel =
            SaveManager.instance.data.level + 1;

        bool unlocked =
            currentLevel > 3 ||
            (currentLevel == 3 &&
             SaveManager.instance.data.undoAnimPlayed == 1);

        if (undoLockImage != null)
            undoLockImage.SetActive(!unlocked);

        foreach (TextMeshProUGUI text in undoText)
        {
            text.text = unlocked
                ? undoCount.ToString()
                : "Lv.3";
        }
    }

    public void RefreshShuffleUI()
    {
        int currentLevel =
            SaveManager.instance.data.level + 1;

        bool unlocked =
            currentLevel > 5 ||
            (currentLevel == 5 &&
             SaveManager.instance.data.shuffleAnimPlayed == 1);

        if (shuffleLockImage != null)
            shuffleLockImage.SetActive(!unlocked);

        foreach (TextMeshProUGUI text in shuffleText)
        {
            text.text = unlocked
                ? shuffleCount.ToString()
                : "Lv.5";
        }
    }

    public void RefreshMagicUI()
    {
        int currentLevel =
            SaveManager.instance.data.level + 1;

        bool unlocked =
            currentLevel > 7 ||
            (currentLevel == 7 &&
             SaveManager.instance.data.magicAnimPlayed == 1);

        if (magicLockImage != null)
            magicLockImage.SetActive(!unlocked);

        foreach (TextMeshProUGUI text in magicText)
        {
            text.text = unlocked
                ? magicCount.ToString()
                : "Lv.7";
        }
    }

    public void CheckUnlockRewards()
    {
        if (SaveManager.instance == null ||
            SaveManager.instance.data == null)
        {
            return;
        }

        // Always sync from save first so we never overwrite saved
        // counts with stale in-memory 0, and unlock flags stay accurate.
        LoadBoosters();

        int currentLevel =
            SaveManager.instance.data.level + 1;

        bool changed = false;

        if (currentLevel >= 3 &&
            SaveManager.instance.data.undoUnlocked == 0)
        {
            undoCount = 3;
            SaveManager.instance.data.undoUnlocked = 1;
            changed = true;

            AnalyticsManager.Instance?.LogBoosterEvent(
                "unlocked",
                "undo",
                3,
                currentLevel,
                "level_unlock"
            );
        }

        if (currentLevel >= 5 &&
            SaveManager.instance.data.shuffleUnlocked == 0)
        {
            shuffleCount = 3;
            SaveManager.instance.data.shuffleUnlocked = 1;
            changed = true;

            AnalyticsManager.Instance?.LogBoosterEvent(
                "unlocked",
                "shuffle",
                3,
                currentLevel,
                "level_unlock"
            );
        }

        if (currentLevel >= 7 &&
            SaveManager.instance.data.magicUnlocked == 0)
        {
            magicCount = 3;
            SaveManager.instance.data.magicUnlocked = 1;
            changed = true;

            AnalyticsManager.Instance?.LogBoosterEvent(
                "unlocked",
                "magic",
                3,
                currentLevel,
                "level_unlock"
            );
        }

        if (changed)
        {
            SaveBoosters();
        }
    }

    public void PlayUnlockAnimationIfNeeded()
    {
        int currentLevel = SaveManager.instance.data.level + 1;

        if (currentLevel == 3 && SaveManager.instance.data.undoAnimPlayed == 0)
        {
            SaveManager.instance.data.undoAnimPlayed = 1;
            SaveManager.instance.SaveData();
            PlayUnlockBounce(undoButtonRect, () =>
            {
                UpdateUI();

                if (undoButtonRect != null)
                {
                    undoButtonRect.DOKill(true);
                    undoButtonRect.localScale = Vector3.one;
                    undoButtonRect.localRotation = Quaternion.identity;
                }

                if (UndoTutorialManager.instance != null)
                {
                    UndoTutorialManager.instance
                        .StartTutorial(undoButtonRect);
                }
            });
        }
        if (currentLevel == 5 && SaveManager.instance.data.shuffleAnimPlayed == 0)
        {
            SaveManager.instance.data.shuffleAnimPlayed = 1;
            SaveManager.instance.SaveData();
            PlayUnlockBounce(shuffleButtonRect, () =>
            {
                UpdateUI();
                if (TutorialManager.instance != null) TutorialManager.instance.StartBoosterTutorial(shuffleButtonRect, "Shuffle");
            });
        }
        if (currentLevel == 7 && SaveManager.instance.data.magicAnimPlayed == 0)
        {
            SaveManager.instance.data.magicAnimPlayed = 1;
            SaveManager.instance.SaveData();
            PlayUnlockBounce(magicButtonRect, () =>
            {
                UpdateUI();
                if (TutorialManager.instance != null) TutorialManager.instance.StartBoosterTutorial(magicButtonRect, "Magic");
            });
        }
    }

    void PlayUnlockBounce(RectTransform rect, System.Action onUnlock)
    {
        if (rect == null) return;

        rect.DOKill(true);
        rect.localRotation = Quaternion.identity;

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(0.6f);
        seq.Append(rect.DOScale(0.8f, 0.25f).SetEase(Ease.OutQuad));
        seq.Join(rect.DOShakeRotation(0.25f, 15f, 20, 90f));
        seq.Append(rect.DOScale(1.45f, 0.35f).SetEase(Ease.OutBack));
        rect.DOPunchRotation(new Vector3(0, 0, 20f), 0.4f, 10, 1f);
        seq.Append(rect.DOScale(1f, 0.2f).SetEase(Ease.InBack));
        seq.Append(rect.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.5f));

        seq.AppendCallback(() =>
       {
           if (SoundManager.instance != null) SoundManager.instance.PlaySound(SoundName.UnlockBooster);
       });

        seq.OnComplete(() =>
        {
            onUnlock?.Invoke();
        });
    }

    public void ShowBoosterLockedMessage(string msg)
    {
        if (lockedBoosterText != null) lockedBoosterText.text = msg;
        ShowMessage(lockedBoosterMessage);
    }

    void ShowMessage(GameObject messageObject)
    {
        if (messageObject == null) return;
        CanvasGroup canvasGroup = messageObject.GetComponent<CanvasGroup>();
        RectTransform rect = messageObject.GetComponent<RectTransform>();

        string seqId = "Msg_" + messageObject.GetInstanceID();
        bool isOpen = messageObject.activeSelf && canvasGroup.alpha > 0.1f;

        DOTween.Kill(seqId);
        rect.DOKill();
        canvasGroup.DOKill();

        messageObject.SetActive(true);
        Vector2 targetPos = new Vector2(rect.anchoredPosition.x, 0f);

        Sequence seq = DOTween.Sequence().SetId(seqId);

        if (isOpen)
        {
            rect.anchoredPosition = targetPos;
            canvasGroup.alpha = 1f;
            rect.localScale = Vector3.one;
            seq.Append(rect.DOPunchScale(Vector3.one * 0.1f, 0.2f, 2, 0.5f));
            seq.Join(rect.DOPunchPosition(Vector3.up * 8f, 0.2f, 5, 0.5f));
        }
        else
        {
            rect.anchoredPosition = new Vector2(targetPos.x, targetPos.y - 20f);
            rect.localScale = Vector3.one;
            canvasGroup.alpha = 0f;

            seq.Append(canvasGroup.DOFade(1f, 0.2f));
            seq.Join(rect.DOAnchorPosY(targetPos.y, 0.3f).SetEase(Ease.OutCubic));
        }

        seq.AppendInterval(1.2f);
        seq.Append(canvasGroup.DOFade(0f, 0.2f));
        seq.Join(rect.DOAnchorPosY(targetPos.y + 15f, 0.2f).SetEase(Ease.InCubic));

        seq.OnComplete(() =>
        {
            rect.anchoredPosition = targetPos;
            messageObject.SetActive(false);
        });
    }

    public bool UseUndo()
    {
        if (undoCount <= 0)
            return false;

        undoCount--;

        SaveBoosters();
        UpdateUI();

        AnalyticsManager.Instance?.LogBoosterEvent(
            action: "used",
            boosterType: "undo",
            amount: 1,
            levelNumber: GetCurrentLevelNumber(),
            source: "gameplay"
        );

        return true;
    }

    public bool UseShuffle()
    {
        if (shuffleCount <= 0)
            return false;

        shuffleCount--;

        SaveBoosters();
        UpdateUI();

        AnalyticsManager.Instance?.LogBoosterEvent(
            action: "used",
            boosterType: "shuffle",
            amount: 1,
            levelNumber: GetCurrentLevelNumber(),
            source: "gameplay"
        );

        return true;
    }

    public bool UseMagic()
    {
        if (magicCount <= 0)
            return false;

        magicCount--;

        SaveBoosters();
        UpdateUI();

        AnalyticsManager.Instance?.LogBoosterEvent(
            action: "used",
            boosterType: "magic",
            amount: 1,
            levelNumber: GetCurrentLevelNumber(),
            source: "gameplay"
        );

        return true;
    }

    public void ShowNothingToUndo() { ShowMessage(nothingToUndoMessage); }
    public void ShowNothingToShuffle() { ShowMessage(nothingToShuffleMessage); }
    public void ShowNothingToMagic() { ShowMessage(nothingToMagicMessage); }

    public void BuyUndo()
    {
        bool purchased =
            CoinManager.instance.SpendCoins(
                1200,
                "undo_pack_3"
            );

        if (!purchased)
        {
            ShowMessage(insuffientCoinUndo);
            return;
        }

        undoCount += 3;

        SaveBoosters();
        UpdateUI();

        AnalyticsManager.Instance?.LogBoosterEvent(
            action: "purchased",
            boosterType: "undo",
            amount: 3,
            levelNumber: GetCurrentLevelNumber(),
            source: "coins"
        );

        UIManager.Instance.HidePopup(
            ScreenType.BuyUndoScreen
        );
    }

    public void BuyShuffle()
    {
        bool purchased =
            CoinManager.instance.SpendCoins(
                1400,
                "shuffle_pack_3"
            );

        if (!purchased)
        {
            ShowMessage(insuffientCoinsShuffle);
            return;
        }

        shuffleCount += 3;

        SaveBoosters();
        UpdateUI();

        AnalyticsManager.Instance?.LogBoosterEvent(
            action: "purchased",
            boosterType: "shuffle",
            amount: 3,
            levelNumber: GetCurrentLevelNumber(),
            source: "coins"
        );

        UIManager.Instance.HidePopup(
            ScreenType.BuyShuffleScreen
        );
    }

    public void BuyMagic()
    {
        bool purchased =
            CoinManager.instance.SpendCoins(
                1800,
                "magic_pack_3"
            );

        if (!purchased)
        {
            ShowMessage(insuffientCoinsMagic);
            return;
        }

        magicCount += 3;

        SaveBoosters();
        UpdateUI();

        AnalyticsManager.Instance?.LogBoosterEvent(
            action: "purchased",
            boosterType: "magic",
            amount: 3,
            levelNumber: GetCurrentLevelNumber(),
            source: "coins"
        );

        UIManager.Instance.HidePopup(
            ScreenType.BuyMagicScreen
        );
    }

    public void AddBoosters(
    int undo,
    int shuffle,
    int magic,
    bool refreshUI = true,
    string source = "reward")
    {
        undo = Mathf.Max(0, undo);
        shuffle = Mathf.Max(0, shuffle);
        magic = Mathf.Max(0, magic);

        if (undo == 0 &&
            shuffle == 0 &&
            magic == 0)
        {
            return;
        }

        if (undo > 0 && CanEarnUndo())
            undoCount += undo;
        else
            undo = 0;

        if (shuffle > 0 && CanEarnShuffle())
            shuffleCount += shuffle;
        else
            shuffle = 0;

        if (magic > 0 && CanEarnMagic())
            magicCount += magic;
        else
            magic = 0;

        if (undo == 0 && shuffle == 0 && magic == 0)
            return;

        SaveBoosters();

        if (refreshUI)
        {
            UpdateUI();
        }

        int levelNumber =
            GetCurrentLevelNumber();

        if (undo > 0)
        {
            AnalyticsManager.Instance?.LogBoosterEvent(
                action: "earned",
                boosterType: "undo",
                amount: undo,
                levelNumber: levelNumber,
                source: source
            );
        }

        if (shuffle > 0)
        {
            AnalyticsManager.Instance?.LogBoosterEvent(
                action: "earned",
                boosterType: "shuffle",
                amount: shuffle,
                levelNumber: levelNumber,
                source: source
            );
        }

        if (magic > 0)
        {
            AnalyticsManager.Instance?.LogBoosterEvent(
                action: "earned",
                boosterType: "magic",
                amount: magic,
                levelNumber: levelNumber,
                source: source
            );
        }
    }

    public void AddUndo(
    int amount,
    bool refreshUI = true,
    string source = "reward")
    {
        if (amount <= 0 || !CanEarnUndo())
            return;

        undoCount += amount;

        SaveBoosters();

        if (refreshUI)
            RefreshUndoUI();

        AnalyticsManager.Instance?.LogBoosterEvent(
            "earned",
            "undo",
            amount,
            GetCurrentLevelNumber(),
            source
        );
    }

    public void AddShuffle(
        int amount,
        bool refreshUI = true,
        string source = "reward")
    {
        if (amount <= 0 || !CanEarnShuffle())
            return;

        shuffleCount += amount;

        SaveBoosters();

        if (refreshUI)
            RefreshShuffleUI();

        AnalyticsManager.Instance?.LogBoosterEvent(
            "earned",
            "shuffle",
            amount,
            GetCurrentLevelNumber(),
            source
        );
    }

    public void AddMagic(
        int amount,
        bool refreshUI = true,
        string source = "reward")
    {
        if (amount <= 0 || !CanEarnMagic())
            return;

        magicCount += amount;

        SaveBoosters();

        if (refreshUI)
            RefreshMagicUI();

        AnalyticsManager.Instance?.LogBoosterEvent(
            "earned",
            "magic",
            amount,
            GetCurrentLevelNumber(),
            source
        );
    }
    private int GetCurrentLevelNumber()
    {
        if (SaveManager.instance == null ||
            SaveManager.instance.data == null)
        {
            return -1;
        }

        return SaveManager.instance.data.level + 1;
    }
}