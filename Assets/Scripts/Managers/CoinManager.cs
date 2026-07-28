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
    public void AddCoins(int amount)
    {
        int startCoins = coins;
        coins += amount;
        SaveCoins();
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount)
            return false;

        int startCoins = coins;
        coins -= amount;
        SaveCoins();
        return true;
    }

    void SaveCoins()
    {
        SaveManager.instance.data.coins = coins;
        SaveManager.instance.SaveData();

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