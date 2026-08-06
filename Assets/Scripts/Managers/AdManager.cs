using UnityEngine;
using GoogleMobileAds.Api;
using System;
using System.Collections;

public class AdManager : MonoBehaviour
{
    public static AdManager instance;

    [Header("Google AdMob Test IDs")]
#if UNITY_ANDROID
    private string bannerId =
           "ca-app-pub-3940256099942544/9214589741";

    private string interstitialId =
        "ca-app-pub-3940256099942544/1033173712";

    private string rewardedId =
        "ca-app-pub-3940256099942544/5224354917";

#elif UNITY_IOS
    private string bannerId =
        "ca-app-pub-3940256099942544/2435281174";

    private string interstitialId =
        "ca-app-pub-3940256099942544/4411468910";

    private string rewardedId =
        "ca-app-pub-3940256099942544/1712485313";

#else
    private string bannerId = "unexpected_platform";
    private string interstitialId = "unexpected_platform";
    private string rewardedId = "unexpected_platform";
#endif

    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;
    private BannerView bannerView;

    private Action pendingRewardCallback;
    private Action pendingFailureCallback;
    private Action pendingInterstitialFinishedCallback;

    private bool rewardEarned = false;
    private bool processAdClosed = false;
    private bool interstitialShowing;


    private bool mobileAdsInitialized;
    private bool bannerShouldBeVisible;

    private Coroutine bannerRestoreCoroutine;

    private string currentBannerPlacement = "gameplay";
    private string currentInterstitialPlacement = "unknown";
    private string currentRewardedPlacement = "unknown";

    private bool bannerAnalyticsVisible;

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            mobileAdsInitialized = true;

            LoadInterstitialAd();
            LoadRewardedAd();

            // Handles the case where UIManager requested a banner
            // before Mobile Ads finished initializing.
            if (bannerShouldBeVisible)
            {
                LoadBannerAd();
            }
        });
    }

    private void Update()
    {
        if (!processAdClosed)
            return;

        processAdClosed = false;

        bool didEarnReward = rewardEarned;

        Action rewardCallback =
            pendingRewardCallback;

        Action failureCallback =
            pendingFailureCallback;

        string rewardedPlacement = currentRewardedPlacement;

        AnalyticsManager.Instance?.LogAdEvent(
    action: didEarnReward
        ? "completed"
        : "closed",
    adType: "rewarded",
    placement: rewardedPlacement,
    levelNumber: GetCurrentLevelNumber(),
    result: didEarnReward
        ? "reward_earned"
        : "no_reward"
);

        // Clear internal state before external callbacks.
        ClearRewardedRequestState();

        if (didEarnReward)
        {
            // PerformRevive runs here.
            // It closes GameOver and requests the banner.
            rewardCallback?.Invoke();
        }
        else
        {
            failureCallback?.Invoke();
        }

        // Prepare the next rewarded ad.
        LoadRewardedAd();

        // Wait until the full-screen native ad has completely closed.
        QueueBannerRestore();
    }

    private void QueueBannerRestore()
    {
        if (bannerRestoreCoroutine != null)
        {
            StopCoroutine(bannerRestoreCoroutine);
        }

        bannerRestoreCoroutine =
            StartCoroutine(RestoreBannerAfterFullscreenAd());
    }

    private IEnumerator RestoreBannerAfterFullscreenAd()
    {
        // Let the rewarded-ad native view finish closing.
        // Realtime is required because Game Over may use timeScale = 0.
        yield return new WaitForSecondsRealtime(0.5f);

        bannerRestoreCoroutine = null;

        if (!bannerShouldBeVisible)
        {
            Debug.Log(
                "[BANNER] Restore cancelled: banner is not requested."
            );
            yield break;
        }

        if (AreNonRewardedAdsRemoved())
        {
            Debug.Log(
                "[BANNER] Restore cancelled: Remove Ads purchased."
            );
            yield break;
        }

        if (!mobileAdsInitialized)
        {
            Debug.Log(
                "[BANNER] Restore cancelled: Mobile Ads not initialized."
            );
            yield break;
        }

        Debug.Log(
            "[BANNER] Destroying stale banner after full-screen ad."
        );

        // Do not use bannerView.Show() here.
        // Fully remove the old native banner.
        ReleaseBannerView();

        // Give Unity/native view removal one frame to complete.
        yield return null;

        if (!bannerShouldBeVisible)
            yield break;

        Debug.Log(
            "[BANNER] Loading fresh banner after full-screen ad."
        );

        LoadBannerAd();
    }

    private void OnDestroy()
    {
        if (instance != this)
            return;

        if (bannerRestoreCoroutine != null)
        {
            StopCoroutine(bannerRestoreCoroutine);
            bannerRestoreCoroutine = null;
        }

        ReleaseBannerView();

        interstitialAd?.Destroy();
        interstitialAd = null;

        rewardedAd?.Destroy();
        rewardedAd = null;

        instance = null;
    }

    // ==========================================
    // BANNER ADS
    // ==========================================

    public void LoadBannerAd()
    {
        if (!mobileAdsInitialized)
        {
            Debug.Log("Banner waiting for Mobile Ads initialization.");
            return;
        }

        if (AreNonRewardedAdsRemoved())
        {
            DestroyBannerAd();
            return;
        }

        // Destroy any previous banner without changing
        // bannerShouldBeVisible.
        ReleaseBannerView();

        // Returns the safe screen width in density-independent pixels.
        int safeWidth = MobileAds.Utils.GetDeviceSafeWidth();

        AdSize adaptiveSize =
            AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(
                safeWidth
            );

        bannerView = new BannerView(
            bannerId,
            adaptiveSize,
            AdPosition.Bottom
        );

        RegisterBannerEvents();

        bannerView.LoadAd(new AdRequest());
    }

    private void RegisterBannerEvents()
    {
        if (bannerView == null)
            return;

        bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner ad loaded.");

            if (AreNonRewardedAdsRemoved())
            {
                DestroyBannerAd();
                return;
            }

            if (bannerShouldBeVisible)
            {
                bannerView?.Show();
                MarkBannerShown();
            }
            else
            {
                bannerView?.Hide();
            }
        };

        bannerView.OnBannerAdLoadFailed += error =>
{
    Debug.LogWarning(
        "Banner ad failed to load: " + error
    );

    bannerAnalyticsVisible = false;

    AnalyticsManager.Instance?.LogAdEvent(
        action: "failed",
        adType: "banner",
        placement: currentBannerPlacement,
        levelNumber: GetCurrentLevelNumber(),
        result: "load_failed"
    );
};

        bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner impression recorded.");
        };

        bannerView.OnAdClicked += () =>
 {
     Debug.Log("Banner clicked.");

     AnalyticsManager.Instance?.LogAdEvent(
         action: "clicked",
         adType: "banner",
         placement: currentBannerPlacement,
         levelNumber: GetCurrentLevelNumber(),
         result: "success"
     );
 };
    }

    public void ShowBannerAd(
    string placement = "gameplay")
    {
        currentBannerPlacement = placement;

        if (AreNonRewardedAdsRemoved())
        {
            DestroyBannerAd();
            return;
        }

        bannerShouldBeVisible = true;

        if (!mobileAdsInitialized)
        {
            return;
        }

        if (bannerView == null ||
            bannerView.IsDestroyed)
        {
            LoadBannerAd();
            return;
        }

        bannerView.Show();
        MarkBannerShown();
    }

    public void HideBannerAd()
    {
        bannerShouldBeVisible = false;
        bannerView?.Hide();
        MarkBannerHidden();
    }

    public void DestroyBannerAd()
    {
        bannerShouldBeVisible = false;
        MarkBannerHidden();
        ReleaseBannerView();
    }

    private void ReleaseBannerView()
    {
        if (bannerView == null)
            return;

        bannerView.Destroy();
        bannerView = null;
    }

    private bool AreNonRewardedAdsRemoved()
    {
        if (PurchaseManager.instance != null &&
            PurchaseManager.instance.IsRemoveAdsPurchased())
        {
            return true;
        }

        return SaveManager.instance != null &&
               SaveManager.instance.data != null &&
               SaveManager.instance.data.removeAdsPurchased == 1;
    }

    // ==========================================
    // INTERSTITIAL ADS
    // ==========================================
    public void LoadInterstitialAd()
    {
        if (AreNonRewardedAdsRemoved())
        {
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
                interstitialAd = null;
            }

            return;
        }

        // Do not destroy an ad that is currently being displayed.
        if (interstitialShowing)
            return;

        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        var adRequest = new AdRequest();

        InterstitialAd.Load(
            interstitialId,
            adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning(
                        "Interstitial failed to load: " + error
                    );

                    return;
                }

                // Remove Ads may have been purchased while loading.
                if (AreNonRewardedAdsRemoved())
                {
                    ad.Destroy();
                    return;
                }

                interstitialAd = ad;

                interstitialAd.OnAdFullScreenContentClosed +=
                    HandleInterstitialClosed;

                interstitialAd.OnAdFullScreenContentFailed +=
                    HandleInterstitialFailed;
            }
        );
    }

    public bool ShowInterstitialAd(
     Action onInterstitialFinished = null,
     string placement = "next_level")
    {
        currentInterstitialPlacement = placement;

        AnalyticsManager.Instance?.LogAdEvent(
            action: "requested",
            adType: "interstitial",
            placement: placement,
            levelNumber: GetCurrentLevelNumber(),
            result: "pending"
        );

        if (AreNonRewardedAdsRemoved())
        {
            Debug.Log(
                "Interstitial skipped because Remove Ads is purchased."
            );

            AnalyticsManager.Instance?.LogAdEvent(
                action: "skipped",
                adType: "interstitial",
                placement: placement,
                levelNumber: GetCurrentLevelNumber(),
                result: "remove_ads"
            );

            return false;
        }

        if (interstitialShowing)
        {
            AnalyticsManager.Instance?.LogAdEvent(
                action: "unavailable",
                adType: "interstitial",
                placement: placement,
                levelNumber: GetCurrentLevelNumber(),
                result: "already_showing"
            );

            return false;
        }

        if (interstitialAd == null ||
            !interstitialAd.CanShowAd())
        {
            Debug.Log("Interstitial is not ready.");

            AnalyticsManager.Instance?.LogAdEvent(
                action: "unavailable",
                adType: "interstitial",
                placement: placement,
                levelNumber: GetCurrentLevelNumber(),
                result: "not_ready"
            );

            LoadInterstitialAd();
            return false;
        }

        pendingInterstitialFinishedCallback =
            onInterstitialFinished;

        interstitialShowing = true;

        interstitialAd.Show();

        AnalyticsManager.Instance?.LogAdEvent(
            action: "shown",
            adType: "interstitial",
            placement: placement,
            levelNumber: GetCurrentLevelNumber(),
            result: "success"
        );

        return true;
    }

    private void HandleInterstitialClosed()
    {
        Debug.Log("Interstitial closed.");

        CompleteInterstitialRequest(
            action: "closed",
            result: "success"
        );
    }

    private void HandleInterstitialFailed(
        AdError error)
    {
        Debug.LogWarning(
            "Interstitial failed to show: " + error
        );

        CompleteInterstitialRequest(
            action: "failed",
            result: "show_failed"
        );
    }

    private void CompleteInterstitialRequest(
     string action,
     string result)
    {
        AnalyticsManager.Instance?.LogAdEvent(
            action: action,
            adType: "interstitial",
            placement: currentInterstitialPlacement,
            levelNumber: GetCurrentLevelNumber(),
            result: result
        );

        Action finishedCallback =
            pendingInterstitialFinishedCallback;

        pendingInterstitialFinishedCallback = null;
        interstitialShowing = false;

        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        currentInterstitialPlacement = "unknown";

        LoadInterstitialAd();

        finishedCallback?.Invoke();
    }

    // ==========================================
    // REWARDED ADS
    // ==========================================
    public void LoadRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        var adRequest = new AdRequest();

        RewardedAd.Load(rewardedId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning(
                        "Rewarded ad failed to load: " + error
                    );
                    return;
                }

                rewardedAd = ad;

                rewardedAd.OnAdFullScreenContentClosed +=
                    HandleRewardedAdClosed;

                rewardedAd.OnAdFullScreenContentFailed +=
                    HandleRewardedAdFailed;
            });
    }

    private void HandleRewardedAdClosed()
    {
        processAdClosed = true;
    }

    private void HandleRewardedAdFailed(
    AdError error)
    {
        Debug.LogWarning(
            "Rewarded ad failed to show: " + error
        );

        AnalyticsManager.Instance?.LogAdEvent(
            action: "failed",
            adType: "rewarded",
            placement: currentRewardedPlacement,
            levelNumber: GetCurrentLevelNumber(),
            result: "show_failed"
        );

        Action failureCallback =
            pendingFailureCallback;

        ClearRewardedRequestState();

        rewardedAd?.Destroy();
        rewardedAd = null;

        failureCallback?.Invoke();

        LoadRewardedAd();
    }

    public bool ShowRewardedAd(
     Action onRewardEarned,
     Action onAdFailedOrClosed = null,
     string placement = "unknown")
    {
        AnalyticsManager.Instance?.LogAdEvent(
            action: "requested",
            adType: "rewarded",
            placement: placement,
            levelNumber: GetCurrentLevelNumber(),
            result: "pending"
        );

        if (rewardedAd != null &&
            rewardedAd.CanShowAd())
        {
            ClearRewardedRequestState();

            currentRewardedPlacement = placement;

            pendingRewardCallback =
                onRewardEarned;

            pendingFailureCallback =
                onAdFailedOrClosed;

            rewardedAd.Show(reward =>
            {
                // Processed later in Update().
                rewardEarned = true;
            });

            AnalyticsManager.Instance?.LogAdEvent(
                action: "shown",
                adType: "rewarded",
                placement: placement,
                levelNumber: GetCurrentLevelNumber(),
                result: "success"
            );

            return true;
        }

        Debug.Log("Rewarded ad is not ready.");

        AnalyticsManager.Instance?.LogAdEvent(
            action: "unavailable",
            adType: "rewarded",
            placement: placement,
            levelNumber: GetCurrentLevelNumber(),
            result: "not_ready"
        );

        LoadRewardedAd();

        return false;
    }

    private void ClearRewardedRequestState()
    {
        pendingRewardCallback = null;
        pendingFailureCallback = null;
        rewardEarned = false;
        processAdClosed = false;
        currentRewardedPlacement = "unknown";
    }

    public bool IsAdsRemoved()
    {
        return SaveManager.instance.data.removeAdsPurchased == 1;
    }

    public void DisableNonRewardedAds()
    {
        DestroyBannerAd();

        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        Debug.Log("Banner and interstitial ads disabled.");
    }

    private int GetCurrentLevelNumber()
    {
        if (SaveManager.instance == null ||
            SaveManager.instance.data == null)
        {
            return -1;
        }

        return SaveManager.instance.data.level + 1;
    }

    private void MarkBannerShown()
    {
        if (bannerAnalyticsVisible)
            return;

        bannerAnalyticsVisible = true;

        AnalyticsManager.Instance?.LogAdEvent(
            action: "shown",
            adType: "banner",
            placement: currentBannerPlacement,
            levelNumber: GetCurrentLevelNumber(),
            result: "success"
        );
    }

    private void MarkBannerHidden()
    {
        if (!bannerAnalyticsVisible)
            return;

        bannerAnalyticsVisible = false;

        AnalyticsManager.Instance?.LogAdEvent(
            action: "hidden",
            adType: "banner",
            placement: currentBannerPlacement,
            levelNumber: GetCurrentLevelNumber(),
            result: "screen_changed"
        );
    }
}