using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;


public class IAPManager : MonoBehaviour
{
    public static IAPManager Instance;

    public bool IsReady { get; private set; }

    private StoreController storeController;

    private readonly Dictionary<string, Product> products =
        new Dictionary<string, Product>();

    public event Action ProductsReady;

    private string activePurchaseProductId =
    string.Empty;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        await InitializeIAP();
    }

    private async System.Threading.Tasks.Task InitializeIAP()
    {
        try
        {
            storeController =
                UnityIAPServices.StoreController();

            // Register callbacks before connecting.
            storeController.OnStoreDisconnected += failure =>
            {
                IsReady = false;

                Debug.LogWarning(
                    "IAP store disconnected: " + failure
                );
            };

            storeController.OnProductsFetched +=
                HandleProductsFetched;

            storeController.OnProductsFetchFailed += failure =>
            {
                IsReady = false;

                Debug.LogError(
                    "IAP product fetch failed: " + failure
                );
            };

            storeController.OnPurchasesFetched +=
                HandlePurchasesFetched;

            storeController.OnPurchasesFetchFailed += failure =>
            {
                Debug.LogWarning(
                    "IAP purchase fetch failed: " + failure
                );
            };

            storeController.OnPurchasePending += HandlePurchasePending;

            storeController.OnPurchaseFailed += HandlePurchaseFailed;

            storeController.OnPurchaseConfirmed += HandlePurchaseConfirmed;

            await storeController.Connect();

            Debug.Log("IAP connected to store.");

            storeController.FetchProducts(
                IAPProductIds.Definitions
            );
        }
        catch (Exception exception)
        {
            IsReady = false;

            Debug.LogException(exception);
        }
    }

    private void HandleProductsFetched(
        List<Product> fetchedProducts)
    {
        products.Clear();

        foreach (Product product in fetchedProducts)
        {
            products[product.definition.id] = product;

            Debug.Log(
                $"IAP product fetched: " +
                $"{product.definition.id}, " +
                $"{product.metadata.localizedPriceString}"
            );
        }

        IsReady = true;

        ProductsReady?.Invoke();

        // Retrieve previous purchases and restore
        // permanent entitlements.
        storeController.FetchPurchases();
    }
    private void HandlePurchasesFetched(
        Orders orders)
    {
        foreach (ConfirmedOrder order in
                 orders.ConfirmedOrders)
        {
            foreach (var item in
                     order.CartOrdered.Items())
            {
                Product product = item.Product;

                if (product.definition.type !=
                    ProductType.NonConsumable)
                {
                    continue;
                }

                string productId =
                    product.definition.id;

                bool newlyRestored =
                    PurchaseManager.instance
                        ?.RestoreNonConsumable(
                            productId
                        ) == true;

                if (newlyRestored)
                {
                    AnalyticsManager.Instance?.LogIapEvent(
                        action: "restored",
                        productId: productId,
                        productType: "nonconsumable"
                    );
                }
            }
        }

        PurchaseManager.instance?.RefreshUI();

        Debug.Log(
            "Existing IAP purchases processed."
        );
    }
    private void HandlePurchasePending(
    PendingOrder order)
    {
        if (PurchaseManager.instance == null)
        {
            Debug.LogError(
                "PurchaseManager is missing. " +
                "Purchase will remain pending."
            );

            return;
        }

        string transactionId =
            order.Info?.TransactionID;

        if (WasTransactionProcessed(transactionId))
        {
            Debug.Log(
                "Transaction was already granted: " +
                transactionId
            );

            foreach (var item in
                     order.CartOrdered.Items())
            {
                string productId =
                    item.Product.definition.id;

                AnalyticsManager.Instance?.LogIapEvent(
                    action: "duplicate_ignored",
                    productId: productId,
                    productType:
                        GetProductTypeName(productId)
                );
            }

            storeController.ConfirmPurchase(order);
            return;
        }

        bool everythingGranted = true;

        foreach (var item in
                 order.CartOrdered.Items())
        {
            string productId =
                item.Product.definition.id;

            string productType =
                GetProductTypeName(productId);

            AnalyticsManager.Instance?.LogIapEvent(
                action: "pending_received",
                productId: productId,
                productType: productType
            );

            bool granted =
                PurchaseManager.instance
                    .GrantNewPurchase(productId);

            if (!granted)
            {
                everythingGranted = false;

                AnalyticsManager.Instance?.LogIapEvent(
                    action: "grant_failed",
                    productId: productId,
                    productType: productType,
                    failureReason:
                        "reward_configuration_missing"
                );

                Debug.LogError(
                    "Unknown or ungrantable product: " +
                    productId
                );

                continue;
            }

            AnalyticsManager.Instance?.LogIapEvent(
                action: "reward_granted",
                productId: productId,
                productType: productType
            );
        }

        if (!everythingGranted)
        {
            // Do not confirm. Unity IAP can return the
            // pending transaction again after it is fixed.
            return;
        }

        MarkTransactionProcessed(transactionId);

        // Persist the transaction ID and all rewards
        // before confirming with the store.
        SaveManager.instance.SaveData();

        storeController.ConfirmPurchase(order);

        Debug.Log(
            "Purchase rewards saved. " +
            "Confirmation requested: " +
            transactionId
        );
    }

    /// <summary>
    /// Manually restore non-consumable purchases (e.g. Remove Ads).
    /// Store callbacks route through HandlePurchasesFetched.
    /// </summary>
    public void RestorePurchases(
        Action<bool, string> onComplete = null)
    {
        if (!IsReady || storeController == null)
        {
            Debug.LogWarning(
                "IAP is not ready yet."
            );

            onComplete?.Invoke(false, "iap_not_ready");
            return;
        }

        storeController.RestoreTransactions(
            (success, message) =>
            {
                if (success)
                {
                    Debug.Log(
                        "Restore purchases completed. " +
                        (message ?? string.Empty)
                    );

                    PurchaseManager.instance?.RefreshUI();
                }
                else
                {
                    Debug.LogWarning(
                        "Restore purchases failed: " +
                        (message ?? "unknown_error")
                    );
                }

                onComplete?.Invoke(success, message);
            }
        );
    }

    public bool BuyProduct(string productId)
    {
        string productType =
            GetProductTypeName(productId);

        if (!IsReady || storeController == null)
        {
            Debug.LogWarning(
                "IAP is not ready yet."
            );

            AnalyticsManager.Instance?.LogIapEvent(
                action: "unavailable",
                productId: productId,
                productType: productType,
                failureReason: "iap_not_ready"
            );

            return false;
        }

        if (!products.ContainsKey(productId))
        {
            Debug.LogWarning(
                "Product was not returned by store: " +
                productId
            );

            AnalyticsManager.Instance?.LogIapEvent(
                action: "unavailable",
                productId: productId,
                productType: productType,
                failureReason: "product_not_fetched"
            );

            return false;
        }

        activePurchaseProductId = productId;

        AnalyticsManager.Instance?.LogIapEvent(
            action: "started",
            productId: productId,
            productType: productType
        );

        storeController.PurchaseProduct(productId);

        return true;
    }

    public string GetLocalizedPrice(
    string productId,
    string fallback = "...")
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            Debug.LogWarning(
                "GetLocalizedPrice received an empty product ID."
            );

            return fallback;
        }

        productId = productId.Trim();

        if (products.TryGetValue(
                productId,
                out Product product))
        {
            return product.metadata.localizedPriceString;
        }

        Debug.LogWarning(
            "Localized price not found for product ID: [" +
            productId +
            "]"
        );

        return fallback;
    }

    private bool WasTransactionProcessed(
        string transactionId)
    {
        if (string.IsNullOrEmpty(transactionId))
            return false;

        List<string> transactions =
            SaveManager.instance.data
                .processedIapTransactionIds;

        return transactions != null &&
               transactions.Contains(transactionId);
    }

    private void MarkTransactionProcessed(
        string transactionId)
    {
        if (string.IsNullOrEmpty(transactionId))
            return;

        if (SaveManager.instance.data
                .processedIapTransactionIds == null)
        {
            SaveManager.instance.data
                .processedIapTransactionIds =
                    new List<string>();
        }

        List<string> transactions =
            SaveManager.instance.data
                .processedIapTransactionIds;

        if (!transactions.Contains(transactionId))
        {
            transactions.Add(transactionId);
        }
    }

    private string GetProductTypeName(
    string productId)
    {
        if (products.TryGetValue(
                productId,
                out Product product))
        {
            return product.definition.type
                .ToString()
                .ToLowerInvariant();
        }

        foreach (ProductDefinition definition in
                 IAPProductIds.Definitions)
        {
            if (definition.id == productId)
            {
                return definition.type
                    .ToString()
                    .ToLowerInvariant();
            }
        }

        return "unknown";
    }

    private string GetFirstProductId(
        Order order)
    {
        if (order == null ||
            order.CartOrdered == null)
        {
            return string.IsNullOrEmpty(
                activePurchaseProductId)
                ? "unknown"
                : activePurchaseProductId;
        }

        foreach (var item in
                 order.CartOrdered.Items())
        {
            if (item.Product != null)
            {
                return item.Product.definition.id;
            }
        }

        return string.IsNullOrEmpty(
            activePurchaseProductId)
            ? "unknown"
            : activePurchaseProductId;
    }

    private void HandlePurchaseFailed(
    FailedOrder failedOrder)
    {
        string productId =
            GetFirstProductId(failedOrder);

        string productType =
            GetProductTypeName(productId);

        string failureReason =
            failedOrder.FailureReason
                .ToString()
                .ToLowerInvariant();

        string action =
            failedOrder.FailureReason ==
            PurchaseFailureReason.UserCancelled
                ? "cancelled"
                : "failed";

        AnalyticsManager.Instance?.LogIapEvent(
            action: action,
            productId: productId,
            productType: productType,
            failureReason: failureReason
        );

        Debug.LogWarning(
            $"IAP purchase {action}: " +
            $"{productId}, Reason: {failureReason}, " +
            $"Details: {failedOrder.Details}"
        );

        activePurchaseProductId = string.Empty;
    }

    private void HandlePurchaseConfirmed(
    Order order)
    {
        foreach (var item in
                 order.CartOrdered.Items())
        {
            string productId =
                item.Product.definition.id;

            AnalyticsManager.Instance?.LogIapEvent(
                action: "confirmed",
                productId: productId,
                productType:
                    GetProductTypeName(productId)
            );

            Debug.Log(
                "IAP purchase confirmed: " +
                productId
            );
        }

        activePurchaseProductId = string.Empty;
    }
}