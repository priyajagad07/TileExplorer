using TMPro;
using UnityEngine;

public class ProfileUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text WorldsVisitedText;

    public static ProfileUI Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        coinsText.text =
            CoinManager.instance.GetCoins().ToString("N0");

        int currentLevel =
            SaveManager.instance.data.level + 1;

        WorldData currentWorld =
            WorldManager.Instance.GetWorldForLevel(currentLevel);

        if (currentWorld != null)
        {
            int visited =
                WorldManager.Instance
                .GetDatabase()
                .worlds
                .IndexOf(currentWorld) + 1;

            WorldsVisitedText.text =
                visited.ToString();
        }
    }
}