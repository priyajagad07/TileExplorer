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
        if (processAdClosed)
        {
            processAdClosed = false;

            if (rewardEarned)
            {
                rewardEarned = false;
                pendingRewardCallback?.Invoke();
            }

            LoadRewardedAd();
        }
    }

    // ==========================================
    // INTERSTITIAL ADS
    // ==========================================
    public void LoadInterstitialAd()
    {
        if (interstitialAd != null) { interstitialAd.Destroy(); interstitialAd = null; }

        var adRequest = new AdRequest();
        InterstitialAd.Load(interstitialId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null) { return; }
            interstitialAd = ad;
        });
    }

    public void ShowInterstitialAd()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
            LoadInterstitialAd();
        }
    }

    // ==========================================
    // REWARDED ADS
    // ==========================================
    public void LoadRewardedAd()
    {
        if (rewardedAd != null) { rewardedAd.Destroy(); rewardedAd = null; }

        var adRequest = new AdRequest();
        RewardedAd.Load(rewardedId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null) { return; }
            rewardedAd = ad;

            rewardedAd.OnAdFullScreenContentClosed += HandleAdClosed;
        });
    }

    private void HandleAdClosed()
    {
        processAdClosed = true;
    }

    public void ShowRewardedAd(Action onRewardEarned)
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            pendingRewardCallback = onRewardEarned;
            rewardEarned = false;

            rewardedAd.Show((Reward reward) =>
            {
                rewardEarned = true;
            });
        }
        else
        {
            Debug.Log("Ad is not ready yet!");
        }
    }
}