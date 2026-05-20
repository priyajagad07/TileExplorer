using TMPro;
using UnityEngine;

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

        if (CoinManager.instance.SpendCoins(2000))
        {
            undoCount += 3;

            SaveBoosters();
            UpdateUI();

            UIManager.Instance.HidePopup(ScreenType.BuyUndoScreen);

            Debug.Log("Undo Purchased");
        }
        else
        {
            insuffientCoinUndo.SetActive(true);
            Debug.Log("Not Enough Coins");
        }
    }
    public void BuyShuffle()
    {
         Debug.Log("Buy Shuffle Clicked");

        if (CoinManager.instance.SpendCoins(2500))
        {
            shuffleCount += 3;

            SaveBoosters();
            UpdateUI();

            UIManager.Instance.HidePopup(ScreenType.BuyShuffleScreen);

            Debug.Log("Shuffle Purchased");
        }
        else
        {
            insuffientCoinsShuffle.SetActive(true);
            Debug.Log("Not Enough Coins");
        }
    }

    public void BuyMagic()
    {
         Debug.Log("Buy Magic Clicked");

        if (CoinManager.instance.SpendCoins(3000))
        {
            magicCount += 3;

            SaveBoosters();
            UpdateUI();

            UIManager.Instance.HidePopup(ScreenType.BuyMagicScreen);

            Debug.Log("Magic Purchased");
        }
        else
        {
            insuffientCoinsMagic.SetActive(true);
            Debug.Log("Not Enough Coins");
        }
    }

    void UpdateUI()
    {
        undoText.text = undoCount.ToString();
        shuffleText.text = shuffleCount.ToString();
        magicText.text = magicCount.ToString();
    }
}