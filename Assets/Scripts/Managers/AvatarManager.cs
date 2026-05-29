using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        selectedAvatar = index;
        avatarPreview.sprite = avatars[index].iconImage.sprite;

        checkMark.transform.SetParent(avatars[index].transform);
        checkMark.transform.localPosition = Vector3.zero;
    }

    public void ConfirmAvatar()
    {   
        PlayerPrefs.SetInt("Avatar", selectedAvatar);
        PlayerPrefs.SetString("PlayerName", nameInput.text);

        PlayerPrefs.Save();

        avatarHomeScreen.sprite = avatars[selectedAvatar].iconImage.sprite;
    }

    void LoadAvatar()
    {
        selectedAvatar = PlayerPrefs.GetInt("Avatar", 0);
        SelectAvatar(selectedAvatar);

        avatarHomeScreen.sprite = avatars[selectedAvatar].iconImage.sprite;
        nameInput.text = PlayerPrefs.GetString("PlayerName", "");
    }
}