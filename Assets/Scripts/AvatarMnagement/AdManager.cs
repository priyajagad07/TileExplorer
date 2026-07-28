using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdManager : MonoBehaviour
{
    public static AdManager instance;

    [Header("Google AdMob Test IDs")]
#if UNITY_ANDROID
    private string interstitialId = "ca-app-pub-3940256099942544/1033173712";
    private string rewardedId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IOS
        private string interstitialId = "ca-app-pub-3940256099942544/4411468910";
        private string rewardedId = "ca-app-pub-3940256099942544/1712485313";
#else
        private string interstitialId = "unexpected_platform";
        private string rewardedId = "unexpected_platform";
#endif  

    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;

    private Action pendingRewardCallback;
    private Action pendingFailureCallback;

    private bool rewardEarned = false;
    private bool processAdClosed = false;


    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            LoadInterstitialAd();
            LoadRewardedAd();
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

    // ==========================================
    // INTERSTITIAL ADS
    // ==========================================
    public void LoadInterstitialAd()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        var adRequest = new AdRequest();

        InterstitialAd.Load(interstitialId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning("Interstitial failed to load: " + error);
                    return;
                }

                interstitialAd = ad;

                interstitialAd.OnAdFullScreenContentClosed += () =>
                {
                    interstitialAd?.Destroy();
                    interstitialAd = null;
                    LoadInterstitialAd();
                };

                interstitialAd.OnAdFullScreenContentFailed += error =>
                {
                    Debug.LogWarning(
                        "Interstitial failed to show: " + error
                    );

                    interstitialAd?.Destroy();
                    interstitialAd = null;
                    LoadInterstitialAd();
                };
            });
    }

    public bool ShowInterstitialAd()
    {
        if (PurchaseManager.instance != null &&
            PurchaseManager.instance.IsRemoveAdsPurchased())
        {
            Debug.Log("Interstitial skipped (Remove Ads purchased).");
            return false;
        }

        if (interstitialAd != null &&
            interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
            return true;
        }

        LoadInterstitialAd();
        return false;
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
}