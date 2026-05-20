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

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void SetProceduralLevel(ProceduralLevelData data)
    {   
        if(data == null)
        {
            Debug.LogError("Procedural data is null");
        }

        if(data.layerLayouts == null || data.layerLayouts.Count == 0)
        {
            Debug.LogError("Procedural level data is missing layer layouts - tiles cannot spawn");
            return;
        }

        proceduralData = data;

        MatchBoardMatch.instance.ResetBoardState();

        BoardSpawner.instance.ClearBoard();
        GenerateProceduralTiles();
    }

    void GenerateProceduralTiles()
    {
        List<GameObject> tilesToSpawn = new List<GameObject>();

        int totalTilesNeeded = GetTotalTilesNeeded();
        Debug.Log("Total tiles Needed: " + totalTilesNeeded);
        Debug.Log("Layer Count: " + proceduralData.layerLayouts.Count);

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

        return count - (count % 3);
    }
}