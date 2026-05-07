using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public static BoardGenerator instance;
    public static int totalTilesInLevel;
    private ProceduralLevelData proceduralData;
    [SerializeField] private GameObject[] tilePrefabs;

    void Awake()
    {
        instance = this;
    }

     public void SetProceduralLevel(ProceduralLevelData data)
    {
        proceduralData = data;

        MatchBoardMatch.instance.ResetBoardState();

        BoardSpawner.instance.ClearBoard();
        GenerateProceduralTiles();
    }

    void GenerateProceduralTiles()
    {
        List<GameObject> tilesToSpawn = new List<GameObject>();

        int totalTilesNeeded = GetValidTileCount(CountOnesInLayout() * proceduralData.layers);
        int typeCount = totalTilesNeeded / 3;

        //tile list (3 of each)
        for (int i = 0; i < typeCount; i++)
        {
            GameObject prefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];

            for (int j = 0; j < 3; j++)
            {
                tilesToSpawn.Add(prefab);
            }
        }

        //shuffle
        for (int i = 0; i < tilesToSpawn.Count; i++)
        {
            GameObject temp = tilesToSpawn[i];
            int randomIndex = Random.Range(i, tilesToSpawn.Count);
            tilesToSpawn[i] = tilesToSpawn[randomIndex];
            tilesToSpawn[randomIndex] = temp;
        }

        totalTilesInLevel = tilesToSpawn.Count;
        BoardSpawner.instance.SpawnTiles(tilesToSpawn, proceduralData);
    }

    int CountOnesInLayout()
    {
        int count = 0;

        foreach (string row in proceduralData.layout)
        {
            foreach (char c in row)
            {
                if (c == '1')
                    count++;
            }
        }
        
        return count;
    }

    int GetValidTileCount(int count)
    {
        return count - (count % 3);
    }
}