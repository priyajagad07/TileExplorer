using System.Collections.Generic;
using UnityEngine;

public class BoardSpawner : MonoBehaviour
{
    public static BoardSpawner instance;

    [SerializeField] private Transform tileParent;

    void Awake()
    {
        instance = this;
    }

    public void SpawnTiles(List<GameObject> tiles, ProceduralLevelData proceduralLevelData)
    {
        int index = 0;

        for (int layer = 0; layer < proceduralLevelData.layers; layer++)
        {
            float layerOffset = layer * 50f;

            float startX = -((proceduralLevelData.cols - 1) * proceduralLevelData.spacing) / 2f;
            float startY = ((proceduralLevelData.rows - 1) * proceduralLevelData.spacing) / 2f;

            for (int row = 0; row < proceduralLevelData.rows; row++)
            {
                string rowData = proceduralLevelData.layout[row];

                for (int col = 0; col < proceduralLevelData.cols; col++)
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

                    float x = startX + col * proceduralLevelData.spacing + layerOffset;
                    float y = startY - row * proceduralLevelData.spacing - layerOffset;
                    rect.anchoredPosition = new Vector2(x, y);

                    obj.transform.SetSiblingIndex(tileParent.childCount);

                    index++;
                }
            }
        }
    }

    public void ClearBoard()
    {
        foreach (Transform child in tileParent)
        {
            Destroy(child.gameObject);
        }
    }

    public Transform GetTileParent()
    {
        return tileParent;
    }
}
