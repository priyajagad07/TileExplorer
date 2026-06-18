using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager instance;

    [SerializeField] private List<TextMeshProUGUI> coinText;
    private int coins;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        LoadCoins();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        SaveCoins();
        UpdateCoinUI();
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount)
            return false;

        coins -= amount;
        SaveCoins();
        UpdateCoinUI();

        return true;
    }

    void SaveCoins()
    {
        SaveManager.instance.data.coins = coins;
        SaveManager.instance.SaveData();
    }

    void UpdateCoinUI()
    {
        foreach (TextMeshProUGUI text in coinText)
        {
            text.text = coins.ToString();
        }
    }

    void LoadCoins()
    {
        coins = SaveManager.instance.data.coins;
        UpdateCoinUI();
    }

    public int GetCoins()
    {
        return coins;
    }
}