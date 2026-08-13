using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One avatar picker cell (e.g. Profile1 … Profile6).
/// Each slot owns its lock overlay + level label under that object.
/// </summary>
public class AvatarSlot : MonoBehaviour
{
    [Header("Data")]
    public int slotIndex;

    [Header("UI")]
    public Image frame;
    public Image avatarImage;
    public GameObject tick;

    [Header("Lock (per-slot, under this Profile object)")]
    public GameObject lockOverlay;
    public TMP_Text lockLevelText;

    private void Awake()
    {
        AutoWireReferences();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        AutoWireReferences();
    }
#endif

    /// <summary>
    /// Finds child objects using your Profile screen naming:
    /// Profile - Image, Selected - Image, Lock - Image, Total - Text (TMP).
    /// </summary>
    public void AutoWireReferences()
    {
        if (frame == null)
        {
            frame = GetComponent<Image>();
        }

        if (avatarImage == null)
        {
            avatarImage = FindChildImage("Profile");
        }

        if (tick == null)
        {
            Transform selected =
                FindChildTransform("Selected");

            if (selected != null)
            {
                tick = selected.gameObject;
            }
        }

        if (lockOverlay == null)
        {
            Transform lockTransform =
                FindChildTransform("Lock");

            if (lockTransform != null)
            {
                lockOverlay = lockTransform.gameObject;
            }
        }

        if (lockLevelText == null && lockOverlay != null)
        {
            lockLevelText =
                lockOverlay.GetComponentInChildren<TMP_Text>(
                    true
                );
        }

        if (lockLevelText == null)
        {
            Transform totalText =
                FindChildTransform("Total");

            if (totalText != null)
            {
                lockLevelText =
                    totalText.GetComponent<TMP_Text>();
            }
        }
    }

    private Transform FindChildTransform(string namePart)
    {
        Transform[] children =
            GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == transform)
                continue;

            if (children[i].name.Contains(namePart))
            {
                return children[i];
            }
        }

        return null;
    }

    private Image FindChildImage(string namePart)
    {
        Image[] images =
            GetComponentsInChildren<Image>(true);

        for (int i = 0; i < images.Length; i++)
        {
            if (images[i].gameObject == gameObject)
                continue;

            if (images[i].name.Contains(namePart))
            {
                return images[i];
            }
        }

        return null;
    }
}
