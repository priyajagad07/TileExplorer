using TMPro;
using UnityEngine;
using DG.Tweening;

public class BoosterManager : MonoBehaviour
{
    public static BoosterManager instance;

    public int undoCount;
    public int shuffleCount;
    public int magicCount;

    [SerializeField] private TextMeshProUGUI undoText;
    [SerializeField] private TextMeshProUGUI shuffleText;
    [SerializeField] private TextMeshProUGUI magicText;

    [SerializeField] private GameObject insuffientCoinUndo;
    [SerializeField] private GameObject insuffientCoinsShuffle;
    [SerializeField] private GameObject insuffientCoinsMagic;

    [SerializeField] private GameObject nothingToUndoMessage;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        LoadBoosters();
        UpdateUI();
    }

    void LoadBoosters()
    {
        undoCount = PlayerPrefs.GetInt("UndoCount", 3);
        shuffleCount = PlayerPrefs.GetInt("ShuffleCount", 3);
        magicCount = PlayerPrefs.GetInt("MagicCount", 3);
    }

    void SaveBoosters()
    {
        PlayerPrefs.SetInt("UndoCount", undoCount);
        PlayerPrefs.SetInt("ShuffleCount", shuffleCount);
        PlayerPrefs.SetInt("MagicCount", magicCount);

        PlayerPrefs.Save();
    }

    public bool UseUndo()
    {
        if (undoCount <= 0)
            return false;

        undoCount--;

        SaveBoosters();
        UpdateUI();

        return true;
    }

    public void ShowNothingToUndo()
    {
        ShowMessage(nothingToUndoMessage);
    }

    public bool UseShuffle()
    {
        if (shuffleCount <= 0)
            return false;

        shuffleCount--;

        SaveBoosters();
        UpdateUI();

        return true;
    }

    public bool UseMagic()
    {
        if (magicCount <= 0)
            return false;

        magicCount--;

        SaveBoosters();
        UpdateUI();

        return true;
    }

    public void BuyUndo()
    {
        Debug.Log("Buy Undo Clicked");

        if (CoinManager.instance.SpendCoins(1200))
        {
            undoCount += 3;

            SaveBoosters();
            UpdateUI();

            UIManager.Instance.HidePopup(ScreenType.BuyUndoScreen);

            Debug.Log("Undo Purchased");
        }
        else
        {
            ShowMessage(insuffientCoinUndo);
            Debug.Log("Not Enough Coins");
        }
    }
    public void BuyShuffle()
    {
        Debug.Log("Buy Shuffle Clicked");

        if (CoinManager.instance.SpendCoins(1400))
        {
            shuffleCount += 3;

            SaveBoosters();
            UpdateUI();

            UIManager.Instance.HidePopup(ScreenType.BuyShuffleScreen);

            Debug.Log("Shuffle Purchased");
        }
        else
        {
            ShowMessage(insuffientCoinsShuffle);
            Debug.Log("Not Enough Coins");
        }
    }

    public void BuyMagic()
    {
        Debug.Log("Buy Magic Clicked");

        if (CoinManager.instance.SpendCoins(1800))
        {
            magicCount += 3;

            SaveBoosters();
            UpdateUI();

            UIManager.Instance.HidePopup(ScreenType.BuyMagicScreen);

            Debug.Log("Magic Purchased");
        }
        else
        {
            ShowMessage(insuffientCoinsMagic);
            Debug.Log("Not Enough Coins");
        }
    }

    void UpdateUI()
    {
        undoText.text = undoCount.ToString();
        shuffleText.text = shuffleCount.ToString();
        magicText.text = magicCount.ToString();
    }

    void ShowMessage(GameObject messageObject)
    {
        messageObject.SetActive(true);
        CanvasGroup canvasGroup = messageObject.GetComponent<CanvasGroup>();
        RectTransform rect = messageObject.GetComponent<RectTransform>();

        rect.DOKill();
        canvasGroup.DOKill();

        Vector2 originalPos = new Vector2(rect.anchoredPosition.x, 0f);

        rect.anchoredPosition = new Vector2(originalPos.x, originalPos.y - 20f);
        rect.localScale = Vector3.one;
        canvasGroup.alpha = 0f;

        Sequence seq = DOTween.Sequence();

        seq.Append(canvasGroup.DOFade(1f, 0.2f));
        seq.Join(rect.DOAnchorPosY(originalPos.y, 0.3f).SetEase(Ease.OutCubic));

        seq.AppendInterval(0.8f);

        seq.Append(canvasGroup.DOFade(0f, 0.2f));
        seq.Join(rect.DOAnchorPosY(originalPos.y + 15f, 0.2f).SetEase(Ease.InCubic));

        seq.OnComplete(() =>
        {
            rect.anchoredPosition = originalPos;
            messageObject.SetActive(false);
        });
    }

    public void AddBoosters(
    int undo,
    int shuffle,
    int magic
)
    {
        undoCount += undo;
        shuffleCount += shuffle;
        magicCount += magic;

        SaveBoosters();
        UpdateUI();
    }
}