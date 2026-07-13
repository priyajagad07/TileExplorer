using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class CoinManager : MonoBehaviour
{
    public static CoinManager instance;

    [SerializeField] private List<TextMeshProUGUI> coinText;
    private int coins;

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
        LoadCoins();
    }

    public void AddCoins(int amount)
    {
        int startCoins = coins;
        coins += amount;
        SaveCoins();
        
        AnimateCoinUI(startCoins, coins); 
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount)
            return false;

        int startCoins = coins;
        coins -= amount;
        SaveCoins();
        
        AnimateCoinUI(startCoins, coins); 

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

    void AnimateCoinUI(int startAmount, int targetAmount)
    {
        DOTween.To(() => startAmount, x => 
        {
            foreach (TextMeshProUGUI text in coinText)
            {
                text.text = x.ToString();
            }
        }, targetAmount, 0.5f).SetEase(Ease.OutQuad);
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