using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.LightImpact);
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

        SaveManager.instance.data.avatarIndex = selectedAvatar;
        SaveManager.instance.data.playerName = playerName;
        SaveManager.instance.SaveData();

        avatarHomeScreen.sprite = avatars[selectedAvatar].iconImage.sprite;
        nameInput.text = playerName;
    }

    void LoadAvatar()
    {
        selectedAvatar = SaveManager.instance.data.avatarIndex;
        SelectAvatar(selectedAvatar);

        avatarHomeScreen.sprite = avatars[selectedAvatar].iconImage.sprite;

        string playerName = SaveManager.instance.data.playerName;

        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Player" + Random.Range(1000, 9999);
            SaveManager.instance.data.playerName = playerName;
            SaveManager.instance.SaveData();
        }

        nameInput.text = playerName;
    }
}