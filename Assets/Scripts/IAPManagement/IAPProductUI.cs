using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IAPProductUI : MonoBehaviour
{
    [Header("Product")]
    [SerializeField] private string productId;

    [Header("UI")]
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button purchaseButton;

    private bool subscribed;

    private void OnEnable()
    {
        SetLoadingState();
        StartCoroutine(WaitForIAPManager());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        Unsubscribe();
    }

    private IEnumerator WaitForIAPManager()
    {
        while (IAPManager.Instance == null)
        {
            yield return null;
        }

        Subscribe();
        Refresh();
    }

    private void Subscribe()
    {
        if (subscribed || IAPManager.Instance == null)
            return;

        IAPManager.Instance.ProductsReady += Refresh;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed)
            return;

        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.ProductsReady -= Refresh;
        }

        subscribed = false;
    }

    private void SetLoadingState()
    {
        if (purchaseButton != null)
        {
            purchaseButton.interactable = false;
        }

        if (priceText != null)
        {
            priceText.text = "...";
        }
    }

    public void Refresh()
    {
        if (IAPManager.Instance == null ||
            !IAPManager.Instance.IsReady)
        {
            SetLoadingState();
            return;
        }

        Debug.Log(
    $"{gameObject.name} requesting price for: [{productId}]"
);

        string localizedPrice =
            IAPManager.Instance.GetLocalizedPrice(
                productId,
                string.Empty
            );

        bool productAvailable =
            !string.IsNullOrEmpty(localizedPrice);

        if (purchaseButton != null)
        {
            purchaseButton.interactable =
                productAvailable;
        }

        if (priceText != null)
        {
            priceText.text = productAvailable
                ? localizedPrice
                : "Unavailable";
        }
    }
}