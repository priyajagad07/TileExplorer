using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public static BoardGenerator instance;
    [SerializeField] private LevelData levelData;
    [SerializeField] private Transform tileParent;
    public static int totalTilesInLevel;
    private ProceduralLevelData proceduralData;
    [SerializeField] private GameObject[] tilePrefabs;

    void Awake()
    {
        instance = this;
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

        SpawnProceduralLayout(tilesToSpawn);
        totalTilesInLevel = tilesToSpawn.Count;
    }

    void SpawnProceduralLayout(List<GameObject> tiles)
    {
        int index = 0;

        for (int layer = 0; layer < proceduralData.layers; layer++)
        {
            float layerOffset = layer * 50f;

            float startX = -((proceduralData.cols - 1) * proceduralData.spacing) / 2f;
            float startY = ((proceduralData.rows - 1) * proceduralData.spacing) / 2f;

            for (int row = 0; row < proceduralData.rows; row++)
            {
                string rowData = proceduralData.layout[row];

                for (int col = 0; col < proceduralData.cols; col++)
                {
                    if (index >= tiles.Count)
                        return;

                    if (rowData[col] != '1')
                        continue;

                    GameObject obj = Instantiate(tiles[index], tileParent);

                    Tile tileScript = obj.GetComponent<Tile>();
                    tileScript.row = row;
                    tileScript.col = col;
                    tileScript.layer = layer;

                    RectTransform rect = obj.GetComponent<RectTransform>();

                    float x = startX + col * proceduralData.spacing + layerOffset;
                    float y = startY - row * proceduralData.spacing - layerOffset;
                    rect.anchoredPosition = new Vector2(x, y);

                    obj.transform.SetSiblingIndex(tileParent.childCount);

                    index++;
                }
            }
        }
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

    public void SetProceduralLevel(ProceduralLevelData data)
    {
        proceduralData = data;

        MatchBoardMatch.instance.ResetBoardState();

        foreach (Transform child in tileParent)
        {
            Destroy(child.gameObject);
        }

        GenerateProceduralTiles();
    }

    int GetValidTileCount(int count)
    {
        return count - (count % 3);
    }
}