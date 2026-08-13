using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Solo.MOST_IN_ONE;

public class SettingsScreenUI : MonoBehaviour
{
    public static SettingsScreenUI instance;

    private const string PrivacyPolicyUrl =
        "https://www.zudolabs.com/privacy-policy";

    [SerializeField] private Button privacyPolicyButton;
    [SerializeField] private Button restorePurchaseButton;
    [SerializeField] private TextMeshProUGUI versionText;

    void Awake()
    {
        instance = this;

        if (privacyPolicyButton == null)
        {
            privacyPolicyButton = FindButton("PP");
        }

        if (restorePurchaseButton == null)
        {
            restorePurchaseButton = FindButton("RP");
        }

        if (versionText == null)
        {
            TextMeshProUGUI[] texts =
                GetComponentsInChildren<TextMeshProUGUI>(true);

            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].gameObject.name == "Version")
                {
                    versionText = texts[i];
                    break;
                }
            }
        }
    }

    void OnEnable()
    {
        RefreshVersionLabel();

        if (privacyPolicyButton != null)
        {
            privacyPolicyButton.onClick.AddListener(OpenPrivacyPolicy);
        }

        if (restorePurchaseButton != null)
        {
            restorePurchaseButton.onClick.AddListener(RestorePurchases);
        }
    }

     void OnDisable()
    {
        if (privacyPolicyButton != null)
        {
            privacyPolicyButton.onClick.RemoveListener(OpenPrivacyPolicy);
        }

        if (restorePurchaseButton != null)
        {
            restorePurchaseButton.onClick.RemoveListener(RestorePurchases);
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public static void RefreshIfOpen()
    {
        if (instance != null)
        {
            instance.RefreshVersionLabel();
        }
    }

    private Button FindButton(string buttonName)
    {
        Button[] buttons =
            GetComponentsInChildren<Button>(true);

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].gameObject.name == buttonName)
            {
                return buttons[i];
            }
        }

        return null;
    }

    /// Opens the Zudo Labs privacy policy in the device browser.
    public void OpenPrivacyPolicy()
    {
        Application.OpenURL(PrivacyPolicyUrl);
    }

    /// Restores non-consumable IAP entitlements (Remove Ads).
    public void RestorePurchases()
    {
        if (IAPManager.Instance == null)
        {
            Debug.LogWarning(
                "Settings: IAPManager is not available."
            );
            return;
        }

        IAPManager.Instance.RestorePurchases(
            (success, message) =>
            {
                if (!success)
                    return;

                if (SoundManager.instance != null)
                {
                    SoundManager.instance.PlayHaptic(
                        MOST_HapticFeedback.HapticTypes.Success
                    );
                }
            }
        );
    }

    public void RefreshVersionLabel()
    {
        if (versionText == null)
            return;

        versionText.text = "Version " + GetDisplayVersion();
    }

    private static string GetDisplayVersion()
    {
        string version = Application.version;

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using AndroidJavaClass unityPlayer =
                new AndroidJavaClass(
                    "com.unity3d.player.UnityPlayer");

            using AndroidJavaObject activity =
                unityPlayer.GetStatic<AndroidJavaObject>(
                    "currentActivity");

            using AndroidJavaObject packageManager =
                activity.Call<AndroidJavaObject>(
                    "getPackageManager");

            using AndroidJavaObject packageInfo =
                packageManager.Call<AndroidJavaObject>(
                    "getPackageInfo",
                    activity.Call<string>("getPackageName"),
                    0);

            int versionCode =
                packageInfo.Get<int>("versionCode");

            version = version + " (" + versionCode + ")";
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning(
                "Settings: Could not read Android version code. " +
                exception.Message
            );
        }
#endif

        return version;
    }
}
