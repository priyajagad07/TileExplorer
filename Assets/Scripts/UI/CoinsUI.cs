using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class CoinsUI : MonoBehaviour
{
    private static readonly List<CoinsUI> instances = new();

    [SerializeField] private TMP_Text coinText;

    private void Awake()
    {
        instances.Add(this);
    }

    private void OnDestroy()
    {
        instances.Remove(this);
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (CoinManager.instance == null)
            return;

        int currentDisplay = 0;

        int.TryParse(
            coinText.text.Replace(",", ""),
            out currentDisplay);

        int target = CoinManager.instance.GetCoins();

        DOTween.To(() => currentDisplay, x =>
        {
            currentDisplay = x;
            coinText.text = x.ToString("N0");

        }, target, .35f);
    }

    public static void RefreshAll()
    {
        foreach (var ui in instances)
            ui.Refresh();
    }
}