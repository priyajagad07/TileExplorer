using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Solo.MOST_IN_ONE;

public class AvatarManager : MonoBehaviour
{
    public static AvatarManager instance;
    [SerializeField] private AvatarSlot[] avatars;
    [SerializeField] private Image avatarPreview;
    [SerializeField] private Image avatarHomeScreen;
    [SerializeField] private GameObject checkMark;
    [SerializeField] private TMP_InputField nameInput;

    private int selectedAvatar = 0;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        LoadAvatar();
    }

    public void SelectAvatar(int index)
    {
        SoundManager.instance.PlayHaptic(
       MOST_HapticFeedback.HapticTypes.LightImpact
                );
        selectedAvatar = index;
        avatarPreview.sprite = avatars[index].iconImage.sprite;

        checkMark.transform.SetParent(avatars[index].transform);
        checkMark.transform.localPosition = Vector3.zero;
    }

    public void ConfirmAvatar()
    {
        string playerName = nameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Player" + Random.Range(1000, 9999);
        }

        PlayerPrefs.SetInt("Avatar", selectedAvatar);
        PlayerPrefs.SetString("PlayerName", playerName);

        PlayerPrefs.Save();

        avatarHomeScreen.sprite = avatars[selectedAvatar].iconImage.sprite;
        nameInput.text = playerName;
    }

    void LoadAvatar()
    {
        selectedAvatar = PlayerPrefs.GetInt("Avatar", 0);
        SelectAvatar(selectedAvatar);

        avatarHomeScreen.sprite = avatars[selectedAvatar].iconImage.sprite;

        string playerName = PlayerPrefs.GetString("PlayerName", "");

        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Player" + Random.Range(1000, 9999);
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();
        }

        nameInput.text = playerName;
    }
}