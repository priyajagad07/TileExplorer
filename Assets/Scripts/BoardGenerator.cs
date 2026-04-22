using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] tilePrefabs;
    [SerializeField] private Transform tileParent;

    [SerializeField] private int totalTiles = 30;

    [SerializeField] private Vector2 spawnAreaMin;
    [SerializeField] private Vector2 spawnAreaMax;

    void Start()
    {
        GenerateTiles();
    }

    void GenerateTiles()
    {
        List<GameObject> tilesToSpawn = new List<GameObject>();

        int typeCount = totalTiles / 3;

        for(int i = 0; i < typeCount; i++)
        {
            GameObject prefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];

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

        //spawm
        foreach(GameObject tile in tilesToSpawn)
        {
            GameObject obj = Instantiate(tile, tileParent);

            RectTransform rect = obj.GetComponent<RectTransform>();

            float x = Random.Range(spawnAreaMin.x, spawnAreaMin.x);
            float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);

            rect.anchoredPosition = new Vector2(x, y);
        }
    }
}