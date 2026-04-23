using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    [SerializeField] private LevelData levelData;
    [SerializeField] private Transform tileParent;

    void Start()
    {
        GenerateTiles();
    }

    void GenerateTiles()
    {
        List<GameObject> tilesToSpawn = new List<GameObject>();

        int typeCount = levelData.totalTiles / 3;

        //tile list (3 of each)
        for(int i = 0; i < typeCount; i++)
        {
            GameObject prefab = levelData.tilePrefabs[Random.Range(0, levelData.tilePrefabs.Length)];

            for(int j = 0; j < 3; j++)
            {
                tilesToSpawn.Add(prefab);
            }
        }

        //shuffle
        for(int i=0; i < tilesToSpawn.Count; i++)
        {
            GameObject temp = tilesToSpawn[i];
            int randomIndex = Random.Range(i, tilesToSpawn.Count);
            tilesToSpawn[i] = tilesToSpawn[randomIndex];
            tilesToSpawn[randomIndex] = temp;
        }

        SpawnGrid(tilesToSpawn);
    }

    void SpawnGrid(List<GameObject> tiles)
    {
        int index = 0;

        float startX = -(levelData.gridCols / 2f) * levelData.spacing;
        float startY = (levelData.gridRows / 2f) * levelData.spacing;

        for(int row = 0; row < levelData.gridRows; row++)
        {
            for (int col = 0; col < levelData.gridCols; col++)
            {
                if(index >= tiles.Count)
                    return;

                GameObject obj = Instantiate(tiles[index], tileParent);

                int randomIndex = Random.Range(0, tileParent.childCount);
                obj.transform.SetSiblingIndex(randomIndex);
                RectTransform rect = obj.GetComponent<RectTransform>();
                
                float x = startX + col * levelData.spacing;
                float y = startY - row * levelData.spacing;

                x += Random.Range(-30f, 30f);
                y += Random.Range(-30f, 30f);

                rect.anchoredPosition = new Vector2(x,y);

                obj.GetComponent<UnityEngine.UI.Image>().canvasRenderer.SetAlpha(1f);
                index++;
            }
        }
    }
}
