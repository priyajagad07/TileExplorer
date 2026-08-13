using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public static BoardGenerator instance;
    public static int totalTilesInLevel;
    private ProceduralLevelData proceduralData;

    [Header("Tile Prefabs")]
    [Tooltip(
        "Order matters. Index 0 unlocks first. " +
        "Later indices unlock as the player progresses."
    )]
    [SerializeField] private GameObject[] tilePrefabs;

    [Header("Progressive Unlock")]
    [Tooltip("How many tile types are available from Level 1.")]
    [SerializeField] private int startingUnlockedCount = 10;

    [Tooltip(
        "A new tile type unlocks every N levels. " +
        "Example: 3 means Level 1-3 use the starting set, " +
        "Level 4 adds tile index 10, Level 7 adds index 11, etc."
    )]
    [SerializeField] private int levelsPerNewTile = 3;

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

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void SetProceduralLevel(ProceduralLevelData data)
    {
        if (data == null)
        {
            Debug.LogError(
                "BoardGenerator: Procedural data is null."
            );
            return;
        }

        if (data.layerLayouts == null ||
            data.layerLayouts.Count == 0)
        {
            Debug.LogError(
                "BoardGenerator: Procedural level data is missing " +
                "layer layouts - tiles cannot spawn."
            );
            return;
        }

        if (BoardSpawner.instance == null)
        {
            Debug.LogError(
                "BoardGenerator: BoardSpawner instance is missing."
            );
            return;
        }

        proceduralData = data;

        if (GameManager.instance != null)
        {
            GameManager.instance.SetLevelDifficulty(
                proceduralData.difficulty
            );
        }

        if (MatchBoardMatch.instance != null)
        {
            MatchBoardMatch.instance.ResetBoardState();
        }

        BoardSpawner.instance.ClearBoard();
        GenerateProceduralTiles();
    }

    void GenerateProceduralTiles()
    {
        List<GameObject> tilesToSpawn = new List<GameObject>();

        int totalTilesNeeded = GetTotalTilesNeeded();

        if (totalTilesNeeded <= 0)
        {
            Debug.LogError(
                "BoardGenerator: No valid tiles available to spawn."
            );
            return;
        }

        GameObject[] availablePrefabs =
            GetAvailableTilePrefabs();

        if (availablePrefabs.Length == 0)
        {
            Debug.LogError(
                "BoardGenerator: No unlocked tile prefabs available."
            );
            return;
        }

        int typeCount = totalTilesNeeded / 3;

        List<GameObject> deepPool = new List<GameObject>();
        List<GameObject> mixedPool = new List<GameObject>();
        List<GameObject> shallowPool = new List<GameObject>();

        float trapProbability = 0f;

        if (proceduralData.difficulty > 1)
        {
            trapProbability =
                (proceduralData.difficulty - 1) * 0.15f;
        }

        Debug.Log(
            "Loaded Level Difficulty is: " +
            proceduralData.difficulty
        );
        Debug.Log(
            "Current Trap Probability is: " +
            trapProbability
        );
        Debug.Log(
            "Unlocked tile types: " +
            availablePrefabs.Length +
            " / " +
            (tilePrefabs != null ? tilePrefabs.Length : 0)
        );

        for (int i = 0; i < typeCount; i++)
        {
            GameObject prefab = availablePrefabs[
                Random.Range(0, availablePrefabs.Length)
            ];

            if (Random.value < trapProbability)
            {
                deepPool.Add(prefab);
                shallowPool.Add(prefab);
                shallowPool.Add(prefab);
            }
            else
            {
                mixedPool.Add(prefab);
                mixedPool.Add(prefab);
                mixedPool.Add(prefab);
            }
        }

        ShuffleList(deepPool);
        ShuffleList(mixedPool);
        ShuffleList(shallowPool);

        tilesToSpawn.AddRange(deepPool);
        tilesToSpawn.AddRange(mixedPool);
        tilesToSpawn.AddRange(shallowPool);

        totalTilesInLevel = tilesToSpawn.Count;
        BoardSpawner.instance.SpawnTiles(
            tilesToSpawn,
            proceduralData
        );

        Debug.Log(
            $"GENERATOR: Expected {totalTilesNeeded} tiles. " +
            $"Sending {tilesToSpawn.Count} tiles to BoardSpawner."
        );
    }

    /// <summary>
    /// Growing pool: Level 1 starts with startingUnlockedCount
    /// types. Every levelsPerNewTile levels, one more prefab
    /// from the array becomes available. Older tiles stay in
    /// the pool. Add new prefabs to the end of tilePrefabs.
    /// </summary>
    private GameObject[] GetAvailableTilePrefabs()
    {
        if (tilePrefabs == null || tilePrefabs.Length == 0)
            return new GameObject[0];

        int displayLevel = GetDisplayLevel();
        int unlockedCount = GetUnlockedTileCount(displayLevel);

        GameObject[] available = new GameObject[unlockedCount];

        for (int i = 0; i < unlockedCount; i++)
        {
            available[i] = tilePrefabs[i];
        }

        return available;
    }

    private int GetUnlockedTileCount(int displayLevel)
    {
        if (tilePrefabs == null || tilePrefabs.Length == 0)
            return 0;

        int startCount = Mathf.Clamp(
            startingUnlockedCount,
            1,
            tilePrefabs.Length
        );

        int perUnlock = Mathf.Max(1, levelsPerNewTile);

        // Level 1..perUnlock → startCount
        // Next band adds +1, and so on.
        int extraUnlocks =
            Mathf.Max(0, displayLevel - 1) / perUnlock;

        return Mathf.Min(
            tilePrefabs.Length,
            startCount + extraUnlocks
        );
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

    void ShuffleList(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GameObject temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    int GetTotalTilesNeeded()
    {
        int count = 0;

        foreach (string[] layout in proceduralData.layerLayouts)
        {
            foreach (string row in layout)
            {
                foreach (char c in row)
                {
                    if (c == '1')
                    {
                        count++;
                    }
                }
            }
        }

        int remainder = count % 3;

        if (remainder != 0)
        {
            int validCount = count - remainder;

            Debug.LogWarning(
                $"Level shape has {count} positions. " +
                $"Spawning {validCount} tiles so the total is divisible by 3."
            );

            return validCount;
        }

        return count;
    }
}
