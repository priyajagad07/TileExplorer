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
        if (CoinManager.instance.SpendCoins(1200))
        {
            undoCount += 3;

            SaveBoosters();
            UpdateUI();
        }
    }
    public void BuyShuffle()
    {
        if (CoinManager.instance.SpendCoins(2500))
        {
            shuffleCount += 3;

            SaveBoosters();
            UpdateUI();
        }
    }

    public void BuyMagic()
    {
        if (CoinManager.instance.SpendCoins(3000))
        {
            magicCount += 3;

            SaveBoosters();
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        undoText.text = undoCount.ToString();
        shuffleText.text = shuffleCount.ToString();
        magicText.text = magicCount.ToString();
    }
}