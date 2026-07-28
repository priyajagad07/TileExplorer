using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    [Header("Lock Sprite")]
    public Sprite lockedCardSprite;

    [Header("World Panels")]
    public List<WorldUIPanel> worldPanels;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        RefreshMap();
    }

    public void RefreshMap()
    {
        if (WorldManager.Instance == null) return;

        int currentLevel = SaveManager.instance.data.level + 1;
        UpdateAllCountries(currentLevel);
    }

   private void UpdateAllCountries(int currentLevel)
    {
        WorldDatabase database = WorldManager.Instance.GetDatabase();
        if (database == null) return;

        int virtualLevel = WorldManager.Instance.GetVirtualLevel(currentLevel);

        foreach (WorldData world in database.worlds)
        {
            WorldUIPanel matchingPanel = worldPanels.Find(p => p.worldData == world);

            if (matchingPanel == null) continue;

            UpdateWorldCards(matchingPanel, world, virtualLevel); // Pass virtualLevel here!
        }
    }

    private void UpdateWorldCards(WorldUIPanel panel, WorldData world, int currentLevel)
    {
        int totalLevels = world.endLevel - world.startLevel + 1;
        int totalCards = world.previewCards.Length;

        float levelsPerCard = (float)totalLevels / totalCards;
        int unlockedCards = 0;

        if (currentLevel >= world.startLevel)
        {
            int levelInsideWorld = currentLevel - world.startLevel;
            int currentIndex = Mathf.FloorToInt(levelInsideWorld / levelsPerCard);
            unlockedCards = currentIndex + 1;
        }

        unlockedCards = Mathf.Clamp(unlockedCards, 0, totalCards);

        for (int i = 0; i < totalCards; i++)
        {
            if (i >= panel.destinationCards.Length) continue;

            DestinationCard destinationCard = panel.destinationCards[i];
            if (destinationCard == null) continue;

            destinationCard.world = world;
            destinationCard.destinationIndex = i;

            bool unlocked = i < unlockedCards;
            destinationCard.SetUnlocked(unlocked);

            if (destinationCard.cityImage != null)
            {
                destinationCard.cityImage.sprite = unlocked ? world.previewCards[i] : lockedCardSprite;
            }
        }
    }
}