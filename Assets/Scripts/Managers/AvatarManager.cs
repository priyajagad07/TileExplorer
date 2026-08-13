using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Solo.MOST_IN_ONE;

public class AvatarManager : MonoBehaviour
{
    public static AvatarManager Instance;

    [Header("Avatar Slots")]
    [SerializeField] private AvatarSlot[] avatarSlots;

    [Header("Unlock Levels (display level)")]
    [SerializeField]
    private int[] unlockLevels =
        { 0, 10, 50, 100, 200, 300 };

    [Header("Frames")]
    [SerializeField] private Sprite normalFrame;
    [SerializeField] private Sprite selectedFrame;

    [Header("Preview")]
    [SerializeField] private Image avatarPreview;

    [Header("Profile")]
    [SerializeField] private TMP_InputField playerNameInput;

    [Header("Home Profile Hint")]
    [Tooltip("Home screen profile button — breathes while an unlock is pending.")]
    [SerializeField] private RectTransform homeProfileButton;

    [Header("Locked Message (optional)")]
    [SerializeField] private GameObject avatarLockedMessage;
    [SerializeField] private TMP_Text avatarLockedText;

    private int selectedAvatar;
    private bool hasUnsavedChanges;
    private bool isBreathing;
    private bool isPlayingUnlockAnims;
    private Tween breathTween;
    private Vector3 homeProfileButtonBaseScale = Vector3.one;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (homeProfileButton != null)
        {
            homeProfileButtonBaseScale =
                homeProfileButton.localScale;
        }

        LoadProfile();
        RefreshUnlockState();
        ResolveLockedMessageText();
    }

    /// <summary>
    /// PopupMessage - Image usually has a TMP child for toast text.
    /// </summary>
    private void ResolveLockedMessageText()
    {
        if (avatarLockedText != null ||
            avatarLockedMessage == null)
        {
            return;
        }

        avatarLockedText =
            avatarLockedMessage
                .GetComponentInChildren<TMP_Text>(true);
    }

    private void OnDisable()
    {
        StopHomeProfileBreath(resetScale: true);
    }

    public void SelectAvatar(int index)
    {
        if (!IsValidSlotIndex(index))
            return;

        if (!IsAvatarUnlocked(index))
        {
            ShowLockedMessage(index);
            return;
        }

        hasUnsavedChanges = true;
        selectedAvatar = index;

        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayHaptic(
                MOST_HapticFeedback.HapticTypes.LightImpact
            );
        }

        UpdateSelectionUI(index);
    }

    private void UpdateSelectionUI(int index)
    {
        if (!IsValidSlotIndex(index))
            return;

        for (int i = 0; i < avatarSlots.Length; i++)
        {
            if (avatarSlots[i] == null)
                continue;

            bool selected = i == index;

            if (avatarSlots[i].frame != null)
            {
                avatarSlots[i].frame.sprite =
                    selected ? selectedFrame : normalFrame;
            }

            if (avatarSlots[i].tick != null)
            {
                avatarSlots[i].tick.SetActive(
                    selected && IsAvatarUnlocked(i)
                );
            }
        }

        if (avatarPreview != null &&
            avatarSlots[index].avatarImage != null)
        {
            avatarPreview.sprite =
                avatarSlots[index].avatarImage.sprite;
        }
    }

    public void ConfirmAvatar()
    {
        hasUnsavedChanges = false;

        if (!IsAvatarUnlocked(selectedAvatar))
        {
            selectedAvatar = 0;
        }

        string playerName = playerNameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName))
            playerName = "Player" + Random.Range(1000, 9999);

        SaveManager.instance.data.avatarIndex = selectedAvatar;
        SaveManager.instance.data.playerName = playerName;

        SaveManager.instance.SaveData();

        AvatarUI.RefreshAll();
        PlayerNameUI.RefreshAll();

        ProfileUI.Instance?.Refresh();
        RefreshUI();
    }

    public void RefreshUI()
    {
        EnsureSaveLists();

        int avatar = SaveManager.instance.data.avatarIndex;

        if (!IsAvatarUnlocked(avatar))
        {
            avatar = 0;
            SaveManager.instance.data.avatarIndex = 0;
            SaveManager.instance.SaveData();
        }

        if (playerNameInput != null)
        {
            playerNameInput.text =
                SaveManager.instance.data.playerName;
        }

        selectedAvatar = avatar;
        RefreshLockUI();
        UpdateSelectionUI(avatar);
    }

    private void LoadProfile()
    {
        EnsureSaveLists();

        selectedAvatar =
            SaveManager.instance.data.avatarIndex;

        RefreshUI();

        AvatarUI.RefreshAll();
        PlayerNameUI.RefreshAll();
    }

    public Sprite GetAvatarSprite(int index)
    {
        if (!IsValidSlotIndex(index))
            return null;

        if (avatarSlots[index] == null ||
            avatarSlots[index].avatarImage == null)
        {
            return null;
        }

        return avatarSlots[index].avatarImage.sprite;
    }

    public bool IsAvatarUnlocked(int index)
    {
        if (!IsValidSlotIndex(index))
            return false;

        int requiredLevel = GetUnlockLevel(index);
        if (requiredLevel <= 0)
            return true;

        return GetDisplayLevel() >= requiredLevel;
    }

    public int GetUnlockLevel(int index)
    {
        if (unlockLevels == null ||
            index < 0 ||
            index >= unlockLevels.Length)
        {
            return index == 0 ? 0 : int.MaxValue;
        }

        return Mathf.Max(0, unlockLevels[index]);
    }

    /// <summary>
    /// Call when Home is shown or level progresses.
    /// Starts/stops profile-button breath for pending unlocks.
    /// </summary>
    public void RefreshUnlockState()
    {
        EnsureSaveLists();
        RefreshLockUI();

        if (HasPendingUnlockAnims())
        {
            StartHomeProfileBreath();
        }
        else
        {
            StopHomeProfileBreath(resetScale: true);
        }
    }

    /// <summary>
    /// Call when Profile UI opens. Plays unlock bounce for
    /// newly unlocked avatars the player hasn't seen yet.
    /// </summary>
    public void PlayPendingUnlockAnimations()
    {
        if (isPlayingUnlockAnims)
            return;

        if (!isActiveAndEnabled)
            return;

        EnsureSaveLists();
        RefreshLockUI();

        List<int> pending = GetPendingUnlockIndices();

        if (pending.Count == 0)
        {
            StopHomeProfileBreath(resetScale: true);
            return;
        }

        isPlayingUnlockAnims = true;
        StopHomeProfileBreath(resetScale: true);

        PlayUnlockSequence(pending, 0);
    }

    /// <summary>
    /// Called when profile screen opens (from ProfileScreenOpener).
    /// </summary>
    public void OnProfileScreenOpened()
    {
        RefreshUI();
        PlayPendingUnlockAnimations();
    }

    private void PlayUnlockSequence(
        List<int> pending,
        int pendingIndex)
    {
        if (pendingIndex >= pending.Count)
        {
            isPlayingUnlockAnims = false;
            SaveManager.instance.SaveData();
            RefreshLockUI();
            StopHomeProfileBreath(resetScale: true);
            return;
        }

        int avatarIndex = pending[pendingIndex];

        if (!IsValidSlotIndex(avatarIndex) ||
            avatarSlots[avatarIndex] == null)
        {
            MarkUnlockAnimSeen(avatarIndex);
            PlayUnlockSequence(pending, pendingIndex + 1);
            return;
        }

        AvatarSlot slot = avatarSlots[avatarIndex];

        if (slot.lockOverlay != null)
        {
            slot.lockOverlay.SetActive(false);
        }

        RectTransform rect =
            slot.transform as RectTransform;

        if (rect == null)
        {
            MarkUnlockAnimSeen(avatarIndex);
            PlayUnlockSequence(pending, pendingIndex + 1);
            return;
        }

        PlayUnlockBounce(rect, () =>
        {
            MarkUnlockAnimSeen(avatarIndex);
            PlayUnlockSequence(pending, pendingIndex + 1);
        });
    }

    private void PlayUnlockBounce(
        RectTransform rect,
        System.Action onUnlock)
    {
        if (rect == null)
        {
            onUnlock?.Invoke();
            return;
        }

        rect.DOKill(true);
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(0.25f);
        seq.Append(
            rect.DOScale(0.8f, 0.25f).SetEase(Ease.OutQuad)
        );
        seq.Join(
            rect.DOShakeRotation(0.25f, 15f, 20, 90f)
        );
        seq.Append(
            rect.DOScale(1.45f, 0.35f).SetEase(Ease.OutBack)
        );
        rect.DOPunchRotation(
            new Vector3(0, 0, 20f),
            0.4f,
            10,
            1f
        );
        seq.Append(
            rect.DOScale(1f, 0.2f).SetEase(Ease.InBack)
        );
        seq.Append(
            rect.DOPunchScale(
                Vector3.one * 0.15f,
                0.3f,
                6,
                0.5f
            )
        );

        seq.AppendCallback(() =>
        {
            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlaySound(
                    SoundName.UnlockBooster
                );
            }
        });

        seq.OnComplete(() =>
        {
            onUnlock?.Invoke();
        });
    }

    private void RefreshLockUI()
    {
        if (avatarSlots == null)
            return;

        for (int i = 0; i < avatarSlots.Length; i++)
        {
            AvatarSlot slot = avatarSlots[i];
            if (slot == null)
                continue;

            slot.AutoWireReferences();

            int slotIndex = GetSlotIndex(i);
            bool unlocked = IsAvatarUnlocked(slotIndex);

            // Each Profile owns its own Lock - Image + Total - Text.
            if (slot.lockOverlay != null)
            {
                slot.lockOverlay.SetActive(!unlocked);
            }

            if (slot.lockLevelText != null)
            {
                int required = GetUnlockLevel(slotIndex);

                if (!unlocked && required > 0)
                {
                    slot.lockLevelText.text =
                        "Lv." + required;
                }
            }
        }
    }

    private int GetSlotIndex(int arrayIndex)
    {
        if (!IsValidSlotIndex(arrayIndex))
            return arrayIndex;

        AvatarSlot slot = avatarSlots[arrayIndex];

        if (slot != null && slot.slotIndex >= 0)
        {
            return slot.slotIndex;
        }

        return arrayIndex;
    }

    private void StartHomeProfileBreath()
    {
        if (homeProfileButton == null)
            return;

        if (isBreathing)
            return;

        isBreathing = true;
        homeProfileButton.DOKill();
        homeProfileButton.localScale =
            homeProfileButtonBaseScale;

        breathTween = homeProfileButton
            .DOScale(
                homeProfileButtonBaseScale * 1.08f,
                0.7f
            )
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    private void StopHomeProfileBreath(bool resetScale)
    {
        isBreathing = false;
        breathTween?.Kill();
        breathTween = null;

        if (homeProfileButton == null)
            return;

        homeProfileButton.DOKill();

        if (resetScale)
        {
            homeProfileButton.localScale =
                homeProfileButtonBaseScale;
        }
    }

    private bool HasPendingUnlockAnims()
    {
        return GetPendingUnlockIndices().Count > 0;
    }

    private List<int> GetPendingUnlockIndices()
    {
        List<int> pending = new List<int>();

        if (avatarSlots == null)
            return pending;

        EnsureSaveLists();

        for (int i = 0; i < avatarSlots.Length; i++)
        {
            if (i == 0)
                continue;

            if (!IsAvatarUnlocked(i))
                continue;

            if (SaveManager.instance.data
                .avatarUnlockAnimsSeen.Contains(i))
            {
                continue;
            }

            pending.Add(i);
        }

        return pending;
    }

    private void MarkUnlockAnimSeen(int index)
    {
        EnsureSaveLists();

        if (!SaveManager.instance.data
            .avatarUnlockAnimsSeen.Contains(index))
        {
            SaveManager.instance.data
                .avatarUnlockAnimsSeen.Add(index);
        }
    }

    private void ShowLockedMessage(int index)
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayHaptic(
                MOST_HapticFeedback.HapticTypes.LightImpact
            );
        }

        int required = GetUnlockLevel(index);
        string msg = "Unlocks at Level " + required + "!";

        if (avatarLockedText != null)
        {
            avatarLockedText.text = msg;
        }

        if (avatarLockedMessage != null)
        {
            ShowToastMessage(avatarLockedMessage);
            return;
        }

        if (BoosterManager.instance != null)
        {
            BoosterManager.instance
                .ShowBoosterLockedMessage(msg);
        }
    }

    private void ShowToastMessage(GameObject messageObject)
    {
        CanvasGroup canvasGroup =
            messageObject.GetComponent<CanvasGroup>();
        RectTransform rect =
            messageObject.GetComponent<RectTransform>();

        if (canvasGroup == null || rect == null)
        {
            messageObject.SetActive(true);
            return;
        }

        string seqId =
            "Msg_" + messageObject.GetInstanceID();
        bool isOpen =
            messageObject.activeSelf &&
            canvasGroup.alpha > 0.1f;

        DOTween.Kill(seqId);
        rect.DOKill();
        canvasGroup.DOKill();

        messageObject.SetActive(true);
        Vector2 targetPos =
            new Vector2(rect.anchoredPosition.x, 0f);

        Sequence seq =
            DOTween.Sequence().SetId(seqId);

        if (isOpen)
        {
            rect.anchoredPosition = targetPos;
            canvasGroup.alpha = 1f;
            rect.localScale = Vector3.one;
            seq.Append(
                rect.DOPunchScale(
                    Vector3.one * 0.1f,
                    0.2f,
                    2,
                    0.5f
                )
            );
        }
        else
        {
            rect.anchoredPosition = new Vector2(
                targetPos.x,
                targetPos.y - 20f
            );
            rect.localScale = Vector3.one;
            canvasGroup.alpha = 0f;

            seq.Append(canvasGroup.DOFade(1f, 0.2f));
            seq.Join(
                rect.DOAnchorPosY(
                    targetPos.y,
                    0.3f
                ).SetEase(Ease.OutCubic)
            );
        }

        seq.AppendInterval(1.2f);
        seq.Append(canvasGroup.DOFade(0f, 0.2f));
        seq.Join(
            rect.DOAnchorPosY(
                targetPos.y + 15f,
                0.2f
            ).SetEase(Ease.InCubic)
        );

        seq.OnComplete(() =>
        {
            rect.anchoredPosition = targetPos;
            messageObject.SetActive(false);
        });
    }

    private void EnsureSaveLists()
    {
        if (SaveManager.instance == null ||
            SaveManager.instance.data == null)
        {
            return;
        }

        if (SaveManager.instance.data
            .avatarUnlockAnimsSeen == null)
        {
            SaveManager.instance.data
                .avatarUnlockAnimsSeen =
                    new List<int>();
        }
    }

    private bool IsValidSlotIndex(int index)
    {
        return avatarSlots != null &&
               index >= 0 &&
               index < avatarSlots.Length;
    }

    private int GetDisplayLevel()
    {
        if (SaveManager.instance == null ||
            SaveManager.instance.data == null)
        {
            return 1;
        }

        return SaveManager.instance.data.level + 1;
    }
}
