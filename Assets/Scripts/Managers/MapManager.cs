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
        Debug.Log("MapManager Start");
        RefreshMap();
    }

    public void RefreshMap()
    {
        Debug.Log("RefreshMap called");
        if (WorldManager.Instance == null) return;

        int currentLevel = SaveManager.instance.data.level + 1;
        UpdateAllCountries(currentLevel);
    }

    private void UpdateAllCountries(int currentLevel)
    {
        Debug.Log("UpdateAllCountries");
        WorldDatabase database = WorldManager.Instance.GetDatabase();
        if (database == null)
        {
            Debug.Log("Database NULL");
            return;
        }

        Debug.Log("World Count = " + database.worlds.Count);
        int virtualLevel = WorldManager.Instance.GetVirtualLevel(currentLevel);

        foreach (WorldData world in database.worlds)
        {
            Debug.Log("Searching panel for " + world.worldName);

            WorldUIPanel matchingPanel = worldPanels.Find(p => p.worldData == world);

            if (matchingPanel == null)
            {
                Debug.Log("NOT FOUND : " + world.worldName);
            }
            else
            {
                Debug.Log("FOUND : " + world.worldName);
            }

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

            Debug.Log(
    $"World: {world.worldName}, Card: {i}, " +
    $"Unlocked: {unlocked}, CurrentLevel: {currentLevel}, " +
    $"UnlockedCards: {unlockedCards}"
);
        }
        Debug.Log($"Current Level: {currentLevel}");
        Debug.Log($"World Start: {world.startLevel}");
        Debug.Log($"World End: {world.endLevel}");
        Debug.Log($"Levels Per Card: {levelsPerCard}");
        Debug.Log($"Unlocked Cards: {unlockedCards}");

    }
}