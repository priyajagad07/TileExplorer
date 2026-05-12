using System.Collections.Generic;
using UnityEngine;

public class BoardSpawner : MonoBehaviour
{
    public static BoardSpawner instance;

    [SerializeField] private RectTransform tileParent;

    void Awake()
    {
        instance = this;
    }

    public void SpawnTiles(List<GameObject> tiles, ProceduralLevelData proceduralLevelData)
    {
        int index = 0;

        float areaWidth = tileParent.rect.width;
        float areaHeight = tileParent.rect.height;

        float spacingX = areaWidth / (proceduralLevelData.cols + 1);
        float spacingY = areaHeight / (proceduralLevelData.rows + 1);

        float spacing = Mathf.Min(spacingX, spacingY);
        spacing = Mathf.Clamp(spacing, 60f, 120f);

        float tileScale = spacing / 100f;
        tileScale = Mathf.Clamp(tileScale, 0.6f, 1f);

        float totalWidth = (proceduralLevelData.cols - 1) * spacing;
        float totalHeight = (proceduralLevelData.rows - 1) * spacing;

        float startX = -totalWidth / 2f;
        float startY = totalHeight / 2f;

        for (int layer = 0; layer < proceduralLevelData.layers; layer++)
        {
            float layerOffsetX = layer * 30f;
            float layerOffsetY = layer * -50f;

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
                    obj.transform.localScale = Vector3.one * tileScale;

                    Tile tileScript = obj.GetComponent<Tile>();
                    tileScript.row = row;
                    tileScript.col = col;
                    tileScript.layer = layer;

                    RectTransform rect = obj.GetComponent<RectTransform>();

                    float x = startX + col * spacing + layerOffsetX;
                    float y = startY - row * spacing + layerOffsetY;

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
