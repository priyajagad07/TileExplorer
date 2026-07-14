using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BoardGenerator : MonoBehaviour
{
    public static BoardGenerator instance;
    public static int totalTilesInLevel;
    private ProceduralLevelData proceduralData;
    [SerializeField] private GameObject[] tilePrefabs;

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
            Debug.LogError("Procedural data is null");
        }

        if (data.layerLayouts == null || data.layerLayouts.Count == 0)
        {
            Debug.LogError("Procedural level data is missing layer layouts - tiles cannot spawn");
            return;
        }

        proceduralData = data;

        if (GameManager.instance != null)
        {
            GameManager.instance.SetLevelDifficulty(proceduralData.difficulty);
        }

        MatchBoardMatch.instance.ResetBoardState();

        BoardSpawner.instance.ClearBoard();
        GenerateProceduralTiles();
    }

    void GenerateProceduralTiles()
    {
        List<GameObject> tilesToSpawn = new List<GameObject>();

        int totalTilesNeeded = GetTotalTilesNeeded();

        if (totalTilesNeeded % 3 != 0)
        {
            Debug.LogError(
                $"Invalid level: {totalTilesNeeded} tiles is not divisible by 3."
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
            trapProbability = (proceduralData.difficulty - 1) * 0.15f;
        }

        Debug.Log("Loaded Level Difficulty is: " + proceduralData.difficulty);
        Debug.Log("Current Trap Probability is: " + trapProbability);

        for (int i = 0; i < typeCount; i++)
        {
            GameObject prefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];

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
        BoardSpawner.instance.SpawnTiles(tilesToSpawn, proceduralData);

        Debug.Log(
    $"GENERATOR: Expected {totalTilesNeeded} tiles. " +
    $"Sending {tilesToSpawn.Count} tiles to BoardSpawner."
);
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

        if (count % 3 != 0)
        {
            int missing = 3 - (count % 3);
            Debug.LogError("🚨 SHAPE BROKEN! Total tiles is " + count + ". You MUST go into the Level Editor and add " + missing + " more tiles, or remove " + (count % 3) + " tiles so it divides evenly by 3!");
        }

        return count;
    }
}