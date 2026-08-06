using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class CoinManager : MonoBehaviour
{
    public static CoinManager instance;
    private int coins;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            LoadCoins();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCoins(
     int amount,
     bool refreshUI = true,
     string source = "unspecified")
    {
        if (amount <= 0)
            return;

        coins += amount;

        SaveCoins(refreshUI);

        AnalyticsManager.Instance?.LogCurrencyEarned(
            currencyName: "coins",
            amount: amount,
            source: source
        );
    }

    public bool SpendCoins(
        int amount,
        string itemName = "unspecified")
    {
        if (amount <= 0 || coins < amount)
            return false;

        coins -= amount;

        SaveCoins();

        AnalyticsManager.Instance?.LogCurrencySpent(
            currencyName: "coins",
            amount: amount,
            itemName: itemName
        );

        return true;
    }

    private void SaveCoins(bool refreshUI = true)
    {
        SaveManager.instance.data.coins = coins;
        SaveManager.instance.SaveData();

        if (refreshUI)
        {
            RefreshCoinsUI();
        }
    }

    public void RefreshCoinsUI()
    {
        CoinsUI.RefreshAll();
        ProfileUI.Instance?.Refresh();
    }

    void LoadCoins()
    {
        coins = SaveManager.instance.data.coins;
    }

    public int GetCoins()
    {
        return coins;
    }
}