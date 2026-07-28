using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Solo.MOST_IN_ONE;

public class AvatarManager : MonoBehaviour
{
    public static AvatarManager Instance;

    [Header("Avatar Slots")]
    [SerializeField] private AvatarSlot[] avatarSlots;

    [Header("Frames")]
    [SerializeField] private Sprite normalFrame;
    [SerializeField] private Sprite selectedFrame;

    [Header("Preview")]
    [SerializeField] private Image avatarPreview;

    [Header("Profile")]
    [SerializeField] private TMP_InputField playerNameInput;

    private int selectedAvatar;
    private bool hasUnsavedChanges;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        LoadProfile();
    }
    public void SelectAvatar(int index)
    {
        hasUnsavedChanges = true;
        selectedAvatar = index;
        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.LightImpact);
        UpdateSelectionUI(index);
    }

    private void UpdateSelectionUI(int index)
    {
        for (int i = 0; i < avatarSlots.Length; i++)
        {
            bool selected = i == index;

            avatarSlots[i].frame.sprite =
                selected ? selectedFrame : normalFrame;

            avatarSlots[i].tick.SetActive(selected);
        }

        avatarPreview.sprite = avatarSlots[index].avatarImage.sprite;
    }

    public void ConfirmAvatar()
    {
        hasUnsavedChanges = false;

        string playerName = playerNameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName))
            playerName = "Player" + Random.Range(1000, 9999);

        SaveManager.instance.data.avatarIndex = selectedAvatar;
        SaveManager.instance.data.playerName = playerName;

        SaveManager.instance.SaveData();

        AvatarUI.RefreshAll();
        ProfileUI.Instance?.Refresh();
        RefreshUI();
    }

    public void RefreshUI()
    {
        int avatar = SaveManager.instance.data.avatarIndex;
        //avatarHome.sprite = avatarSlots[avatar].avatarImage.sprite;
        playerNameInput.text = SaveManager.instance.data.playerName;
        selectedAvatar = avatar;
        UpdateSelectionUI(avatar);
    }

    private void LoadProfile()
    {
        if (string.IsNullOrEmpty(SaveManager.instance.data.playerName))
        {
            SaveManager.instance.data.playerName =
                "Player" + Random.Range(1000, 9999);

            SaveManager.instance.SaveData();
        }

        selectedAvatar = SaveManager.instance.data.avatarIndex;

        RefreshUI();
    }

    public Sprite GetAvatarSprite(int index)
    {
        if (index < 0 || index >= avatarSlots.Length)
            return null;

        return avatarSlots[index].avatarImage.sprite;
    }
}