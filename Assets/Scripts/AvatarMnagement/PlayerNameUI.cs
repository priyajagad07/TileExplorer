using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class PlayerNameUI : MonoBehaviour
{
    private static readonly List<PlayerNameUI> instances = new();

    [SerializeField] private TMP_Text playerNameText;

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
        playerNameText.text = SaveManager.instance.data.playerName;
    }

    public static void RefreshAll()
    {
        foreach (var ui in instances)
            ui.Refresh();
    }
}