using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance;

    [SerializeField]
    private WorldDatabase worldDatabase;

    [Header("Endgame Settings")]
    [Tooltip("If true, Level 601 loops back to the first world's background. If false, it stays on the final background forever.")]
    public bool loopCountriesInfinitely = true; 

    private void Awake()
    {
        Instance = this;
    }

    public int GetVirtualLevel(int realLevel)
    {
        if (worldDatabase == null || worldDatabase.worlds.Count == 0) return realLevel;

        int maxLevel = 0;
        foreach (WorldData world in worldDatabase.worlds)
        {
            if (world.endLevel > maxLevel)
            {
                maxLevel = world.endLevel;
            }
        }

        if (realLevel <= maxLevel) return realLevel;

        if (loopCountriesInfinitely)
        {
            return ((realLevel - 1) % maxLevel) + 1;
        }
        else
        {
            return maxLevel; 
        }
    }

    public WorldData GetWorldForLevel(int level)
    {
        int virtualLevel = GetVirtualLevel(level);

        foreach (WorldData world in worldDatabase.worlds)
        {
            if (virtualLevel >= world.startLevel && virtualLevel <= world.endLevel)
            {
                return world;
            }
        }
        return null;
    }

    public WorldDatabase GetDatabase()
    {
        return worldDatabase;
    }

    public bool IsWorldChanging()
    {
        int nextLevel = SaveManager.instance.data.level + 1;
        WorldData nextWorld = GetWorldForLevel(nextLevel);
        return nextWorld != BackgroundManager.Instance.GetCurrentWorld();
    }

    public WorldData GetNextWorld()
    {
        int nextLevel = SaveManager.instance.data.level + 1;
        return GetWorldForLevel(nextLevel);
    }
}