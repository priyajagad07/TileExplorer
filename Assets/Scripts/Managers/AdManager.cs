using UnityEngine;
using UnityEngine.Advertisements;
using System;

public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdManager instance;

    [Header("Ad IDs")]
    [SerializeField] private string androidGameId = "4859013"; 
    [SerializeField] private string iOSGameId = "4859012";
    [SerializeField] private bool testMode = true;

    [Header("Ad Unit Names")]
    [SerializeField] private string rewardedAdUnitId = "Rewarded_Android";
    [SerializeField] private string interstitialAdUnitId = "Interstitial_Android";

    private string gameId;
    private Action onRewardedAdSuccess;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeAds();
    }

    public void InitializeAds()
    {
        gameId = (Application.platform == RuntimePlatform.IPhonePlayer) ? iOSGameId : androidGameId;
        
        rewardedAdUnitId = (Application.platform == RuntimePlatform.IPhonePlayer) ? "Rewarded_iOS" : "Rewarded_Android";
        interstitialAdUnitId = (Application.platform == RuntimePlatform.IPhonePlayer) ? "Interstitial_iOS" : "Interstitial_Android";

        Advertisement.Initialize(gameId, testMode, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        LoadRewardedAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    public void LoadRewardedAd()
    {
        Debug.Log("Loading Rewarded Ad...");
        Advertisement.Load(rewardedAdUnitId, this);
    }

    public void ShowRewardedAd(Action onSuccess)
    {
        onRewardedAdSuccess = onSuccess;
        Advertisement.Show(rewardedAdUnitId, this);
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(rewardedAdUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Player watched the whole ad! Giving reward...");
            
            if (onRewardedAdSuccess != null)
            {
                onRewardedAdSuccess.Invoke();
                onRewardedAdSuccess = null;
            }
        }
        
        LoadRewardedAd(); 
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }
    public void OnUnityAdsAdLoaded(string adUnitId) { }
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message) { }
}