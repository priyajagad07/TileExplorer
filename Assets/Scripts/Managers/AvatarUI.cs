using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarUI : MonoBehaviour
{
    private static readonly List<AvatarUI> instances = new();

    [SerializeField] private Image avatarImage;

    private void Awake()
    {
        instances.Add(this);
    }

    private void OnDestroy()
    {
        instances.Remove(this);
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (AvatarManager.Instance == null)
            return;

        int avatarIndex = SaveManager.instance.data.avatarIndex;

        avatarImage.sprite =
            AvatarManager.Instance.GetAvatarSprite(avatarIndex);
    }

    public static void RefreshAll()
    {
        foreach (var ui in instances)
            ui.Refresh();
    }
}