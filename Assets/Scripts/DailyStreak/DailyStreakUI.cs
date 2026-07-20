using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class DailyStreakUI : MonoBehaviour
{
    public static DailyStreakUI instance;

    [SerializeField] private DaySlotUI[] daySlots;
    [SerializeField] private GameObject rewardPopup;
    [SerializeField] private TextMeshProUGUI rewardText;

    [SerializeField] private GameObject rewardPreviewPopup;
    [SerializeField] private RewardSlotUI[] rewardSlots;

    [Header("Day Tabs")]
    [SerializeField] private Sprite activeTabSprite;
    [SerializeField] private Color activeTextColor = Color.white;
    [SerializeField] private Color inactiveTextColor = new Color32(92, 62, 163, 255);

    [Header("Chest")]
    [SerializeField] private Sprite lockedChestSprite;
    [SerializeField] private Sprite claimedChestSprite;

    [SerializeField] private float normalChestSize = 120f;
    [SerializeField] private float claimedChestSize = 160f;
    [SerializeField] private float popChestSize = 170f;
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
        rewardPopup.SetActive(false);
    }

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        int streak = DailyStreakManager.instance.GetStreak();

        for (int i = 0; i < daySlots.Length; i++)
        {
            bool claimed = i < streak;

            if (claimed)
            {
                daySlots[i].background.gameObject.SetActive(true);
                daySlots[i].background.sprite = activeTabSprite;
                daySlots[i].dayText.color = activeTextColor;
            }
            else
            {
                daySlots[i].background.gameObject.SetActive(false);
                daySlots[i].dayText.color = inactiveTextColor;
            }

            daySlots[i].chest.sprite =
                claimed ? claimedChestSprite : lockedChestSprite;

            RectTransform rect = daySlots[i].chest.rectTransform;

            rect.sizeDelta = new Vector2(
                claimed ? claimedChestSize : normalChestSize,
                claimed ? claimedChestSize : normalChestSize
            );
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
        }
    }
    public void OpenFromHome()
    {
        openedAfterReward = false;

        Refresh();
    }
}