using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    [SerializeField] private LevelData levelData;
    [SerializeField] private Transform tileParent;
    public static int totalTilesInLevel;

    // void Start()
    // {
    //     GenerateTiles();
    // }

    void GenerateTiles()
    {
        List<GameObject> tilesToSpawn = new List<GameObject>();

        int totalTilesNeeded = CountOnesInLayout() * levelData.layers;
        int typeCount = totalTilesNeeded / 3;

        //tile list (3 of each)
        for (int i = 0; i < typeCount; i++)
        {
            GameObject prefab = levelData.tilePrefabs[Random.Range(0, levelData.tilePrefabs.Length)];

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

        SpawnLayout(tilesToSpawn);
        totalTilesInLevel = tilesToSpawn.Count;
    }

    void SpawnLayout(List<GameObject> tiles)
    {
        int index = 0;

        for (int layer = 0; layer < levelData.layers; layer++)
        {
            float layerOffset = layer * 10f;

            float startX = -(levelData.gridCols / 2f) * levelData.spacing;
            float startY = (levelData.gridRows / 2f) * levelData.spacing;

            for (int row = 0; row < levelData.gridRows; row++)
            {
                string rowData = levelData.layout[row];

                for (int col = 0; col < levelData.gridCols; col++)
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

                    float x = startX + col * levelData.spacing + layerOffset;
                    float y = startY - row * levelData.spacing - layerOffset;
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

        foreach (string row in levelData.layout)
        {
            foreach (char c in row)
            {
                if (c == '1')
                    count++;
            }
        }
        return count;
    }

    public void SetLevel(LevelData newLevel)
    {
        levelData = newLevel;

        foreach(Transform child in tileParent)
        {
            Destroy(child.gameObject);
        }

        GenerateTiles();
    }
}