using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class DailyStreakUI : MonoBehaviour
{
    public static DailyStreakUI instance;

    [SerializeField] private TextMeshProUGUI streakText;
    [SerializeField] private Image[] dayIcons;
    [SerializeField] private Color completedColor;
    [SerializeField] private Color defaultColor;

    [SerializeField] private Transform birdParent;
    [SerializeField] private Transform keepGoingButton;

    [SerializeField] private GameObject rewardPopup;
    [SerializeField] private TextMeshProUGUI rewardText;

    [SerializeField] private GameObject rewardPreviewPopup;
    [SerializeField] private RewardSlotUI[] rewardSlots;

    public bool openedAfterReward = false;

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
        keepGoingButton.localScale = Vector3.zero;
        rewardPopup.SetActive(false);
    }

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        int streak = DailyStreakManager.instance.GetStreak();

        for (int i = 0; i < dayIcons.Length; i++)
        {
            dayIcons[i].color = i < streak ? completedColor : defaultColor;
        }
    }

    public void OpenDailyReward()
    {
        Debug.Log("OpenDailyReward");

        openedAfterReward = true;

        if (DailyStreakManager.instance.HasPendingReward())
        {
            Debug.Log("Has Pending Reward");
            PlayRewardSequence();
        }
        else
        {
            Debug.Log("No Pending Reward");

            Refresh();

            streakText.text = DailyStreakManager.instance.GetStreak().ToString();
        }
    }
    public void OpenFromHome()
    {
        openedAfterReward = false;

        Refresh();

        streakText.text =
            DailyStreakManager.instance
                .GetStreak()
                .ToString();

        keepGoingButton.localScale =
            Vector3.one;
    }
}