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

    void Awake()
    {
        instance = this;
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
        SaveManager.instance.data.undoCount = undoCount;
        SaveManager.instance.data.shuffleCount = shuffleCount;
        SaveManager.instance.data.magicCount = magicCount;
        SaveManager.instance.SaveData();
    }

    public void UpdateUI()
    {
        int currentLevel = SaveManager.instance.data.level + 1;

        bool undoUnlocked = currentLevel > 3 || (currentLevel == 3 && SaveManager.instance.data.undoAnimPlayed == 1);
        if (undoLockImage != null) undoLockImage.SetActive(!undoUnlocked);
        foreach (TextMeshProUGUI undo in undoText) undo.text = undoUnlocked ? undoCount.ToString() : "Lv.3";

        bool shuffleUnlocked = currentLevel > 5 || (currentLevel == 5 && SaveManager.instance.data.shuffleAnimPlayed == 1);
        if (shuffleLockImage != null) shuffleLockImage.SetActive(!shuffleUnlocked);
        foreach (TextMeshProUGUI shuffle in shuffleText) shuffle.text = shuffleUnlocked ? shuffleCount.ToString() : "Lv.5";

        bool magicUnlocked = currentLevel > 7 || (currentLevel == 7 && SaveManager.instance.data.magicAnimPlayed == 1);
        if (magicLockImage != null) magicLockImage.SetActive(!magicUnlocked);
        foreach (TextMeshProUGUI magic in magicText) magic.text = magicUnlocked ? magicCount.ToString() : "Lv.7";
    }

    public void CheckUnlockRewards()
    {
        int currentLevel = SaveManager.instance.data.level + 1;

        if (currentLevel >= 3 && SaveManager.instance.data.undoUnlocked == 0)
        {
            undoCount += 3;
            SaveManager.instance.data.undoUnlocked = 1;
        }
        if (currentLevel >= 5 && SaveManager.instance.data.shuffleUnlocked == 0)
        {
            shuffleCount += 3;
            SaveManager.instance.data.shuffleUnlocked = 1;
        }
        if (currentLevel >= 7 && SaveManager.instance.data.magicUnlocked == 0)
        {
            magicCount += 3;
            SaveManager.instance.data.magicUnlocked = 1;
        }
        SaveBoosters();
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
                if (TutorialManager.instance != null) TutorialManager.instance.StartBoosterTutorial(undoButtonRect, "Undo");
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

    public bool UseUndo() { if (undoCount <= 0) return false; undoCount--; SaveBoosters(); UpdateUI(); return true; }
    public bool UseShuffle() { if (shuffleCount <= 0) return false; shuffleCount--; SaveBoosters(); UpdateUI(); return true; }
    public bool UseMagic() { if (magicCount <= 0) return false; magicCount--; SaveBoosters(); UpdateUI(); return true; }

    public void ShowNothingToUndo() { ShowMessage(nothingToUndoMessage); }
    public void ShowNothingToShuffle() { ShowMessage(nothingToShuffleMessage); }
    public void ShowNothingToMagic() { ShowMessage(nothingToMagicMessage); }

    public void BuyUndo() { if (CoinManager.instance.SpendCoins(1200)) { undoCount += 3; SaveBoosters(); UpdateUI(); UIManager.Instance.HidePopup(ScreenType.BuyUndoScreen); } else ShowMessage(insuffientCoinUndo); }
    public void BuyShuffle() { if (CoinManager.instance.SpendCoins(1400)) { shuffleCount += 3; SaveBoosters(); UpdateUI(); UIManager.Instance.HidePopup(ScreenType.BuyShuffleScreen); } else ShowMessage(insuffientCoinsShuffle); }
    public void BuyMagic() { if (CoinManager.instance.SpendCoins(1800)) { magicCount += 3; SaveBoosters(); UpdateUI(); UIManager.Instance.HidePopup(ScreenType.BuyMagicScreen); } else ShowMessage(insuffientCoinsMagic); }

    public void AddBoosters(int undo, int shuffle, int magic) { undoCount += undo; shuffleCount += shuffle; magicCount += magic; SaveBoosters(); UpdateUI(); }
    public void AddUndo(int amount) { undoCount += amount; SaveBoosters(); UpdateUI(); }
    public void AddShuffle(int amount) { shuffleCount += amount; SaveBoosters(); UpdateUI(); }
    public void AddMagic(int amount) { magicCount += amount; SaveBoosters(); UpdateUI(); }
}