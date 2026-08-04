using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class PlayerNameUI : MonoBehaviour
{
    private static readonly List<PlayerNameUI> instances = new();

    [SerializeField] private TMP_Text playerNameText;

    private void Awake()
    {
        if (!instances.Contains(this))
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
        if (playerNameText == null ||
            SaveManager.instance == null ||
            SaveManager.instance.data == null)
        {
            return;
        }

        playerNameText.text = SaveManager.instance.data.playerName;
    }

    public static void RefreshAll()
    {
        for (int i = instances.Count - 1; i >= 0; i--)
        {
            if (instances[i] == null)
            {
                instances.RemoveAt(i);
                continue;
            }

            instances[i].Refresh();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instances.Clear();
    }
}