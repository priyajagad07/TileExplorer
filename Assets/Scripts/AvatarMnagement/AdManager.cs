using UnityEngine;
using GoogleMobileAds.Api;
using System;

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

    void Update()
    {
        if (!processAdClosed)
            return;

        processAdClosed = false;

        bool didEarnReward = rewardEarned;

        Action rewardCallback =
            pendingRewardCallback;

        Action failureCallback =
            pendingFailureCallback;

        // Clear the request before invoking external code.
        // The callback may start another ad or change scenes.
        ClearRewardedRequestState();

        if (didEarnReward)
        {
            rewardCallback?.Invoke();
        }
        else
        {
            failureCallback?.Invoke();
        }

        LoadRewardedAd();
    }

    private void OnDestroy()
    {
        if (instance != this)
            return;

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
        };

        bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner impression recorded.");
        };

        bannerView.OnAdClicked += () =>
        {
            Debug.Log("Banner clicked.");
        };
    }

    public void ShowBannerAd()
    {
        if (AreNonRewardedAdsRemoved())
        {
            Debug.Log("Banner skipped because Remove Ads is purchased.");
            DestroyBannerAd();
            return;
        }

        bannerShouldBeVisible = true;

        if (!mobileAdsInitialized)
        {
            // Start() loads it after initialization.
            return;
        }

        if (bannerView == null || bannerView.IsDestroyed)
        {
            LoadBannerAd();
            return;
        }

        bannerView.Show();
    }
    public void HideBannerAd()
    {
        bannerShouldBeVisible = false;
        bannerView?.Hide();
    }

    public void DestroyBannerAd()
    {
        bannerShouldBeVisible = false;
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
        Action onInterstitialFinished = null)
    {
        if (AreNonRewardedAdsRemoved())
        {
            Debug.Log(
                "Interstitial skipped because Remove Ads is purchased."
            );

            return false;
        }

        if (interstitialShowing)
        {
            Debug.Log("An interstitial is already showing.");
            return false;
        }

        if (interstitialAd == null ||
            !interstitialAd.CanShowAd())
        {
            Debug.Log("Interstitial is not ready.");

            LoadInterstitialAd();
            return false;
        }

        pendingInterstitialFinishedCallback =
            onInterstitialFinished;

        interstitialShowing = true;

        interstitialAd.Show();

        return true;
    }

    private void HandleInterstitialClosed()
    {
        Debug.Log("Interstitial closed.");

        CompleteInterstitialRequest();
    }

    private void HandleInterstitialFailed(AdError error)
    {
        Debug.LogWarning(
            "Interstitial failed to show: " + error
        );

        CompleteInterstitialRequest();
    }

    private void CompleteInterstitialRequest()
    {
        Action finishedCallback =
            pendingInterstitialFinishedCallback;

        pendingInterstitialFinishedCallback = null;
        interstitialShowing = false;

        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        // Prepare the next interstitial.
        LoadInterstitialAd();

        // Continue to the next level after the ad closes or fails.
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
    AdError error
)
    {
        Debug.LogWarning(
            "Rewarded ad failed to show: " + error
        );

        Action failureCallback =
            pendingFailureCallback;

        ClearRewardedRequestState();

        rewardedAd?.Destroy();
        rewardedAd = null;

        // Invoke after internal state is clean.
        failureCallback?.Invoke();

        LoadRewardedAd();
    }

    public bool ShowRewardedAd(
    Action onRewardEarned,
    Action onAdFailedOrClosed = null)
    {
        if (rewardedAd != null &&
    rewardedAd.CanShowAd())
        {
            // Clear any stale state from a previous rewarded ad.
            ClearRewardedRequestState();

            pendingRewardCallback = onRewardEarned;
            pendingFailureCallback = onAdFailedOrClosed;

            rewardedAd.Show(reward =>
            {
                rewardEarned = true;
            });

            return true;
        }

        Debug.Log("Rewarded ad is not ready.");

        LoadRewardedAd();

        return false;
    }

    private void ClearRewardedRequestState()
    {
        pendingRewardCallback = null;
        pendingFailureCallback = null;
        rewardEarned = false;
        processAdClosed = false;
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
}