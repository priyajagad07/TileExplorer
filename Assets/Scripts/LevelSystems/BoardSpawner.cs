using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardSpawner : MonoBehaviour
{
    public static BoardSpawner instance;

    [SerializeField] private RectTransform tileParent;

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
    
    public void SpawnTiles(List<GameObject> tiles, ProceduralLevelData proceduralLevelData)
    {
        if (proceduralLevelData == null || tiles == null || tiles.Count == 0)
            return;

        int index = 0;

        float areaWidth = tileParent.rect.width;
        float areaHeight = tileParent.rect.height;

        float spacingX = areaWidth / (proceduralLevelData.cols + 1);
        float spacingY = areaHeight / (proceduralLevelData.rows + 1);

        float spacing = Mathf.Min(spacingX, spacingY);
        spacing = Mathf.Clamp(spacing, 85f, 150f);
        spacing *= 1.08f;

        float tileScale = spacing / 100f;
        tileScale = Mathf.Clamp(tileScale, 0.65f, 0.9f);

        for (int layer = 0; layer < proceduralLevelData.layerLayouts.Count; layer++)
        {
            float layerOffsetY = layer * -55f;
            float layerOffsetX = 0f;

            if (layer % 2 == 1)
            {
                layerOffsetX = -40f;
            }
            else if (layer > 0)
            {
                layerOffsetX = 40f;
            }

            string[] currentLayout = proceduralLevelData.layerLayouts[layer];

            int currentRows = currentLayout.Length;
            int currentCols = currentLayout[0].Length;

            float currentWidth = (currentCols - 1) * spacing;
            float currentHeight = (currentRows - 1) * spacing;

            float currentStartX = -currentWidth / 2f;
            float currentStartY = currentHeight / 2f;

            for (int row = 0; row < currentLayout.Length; row++)
            {
                string rowData = currentLayout[row];

                for (int col = 0; col < currentCols; col++)
                {
                    if (index >= tiles.Count)
                        break;

                    if (string.IsNullOrEmpty(rowData) || col >= rowData.Length)
                        continue;

                    if (rowData[col] != '1')
                        continue;

                    Debug.Log("Spawning Tile Index: " + index);

                    GameObject obj = Instantiate(tiles[index], tileParent);
                    obj.transform.localScale = Vector3.one * tileScale;

                    Tile tileScript = obj.GetComponent<Tile>();
                    tileScript.row = row;
                    tileScript.col = col;
                    tileScript.layer = layer;

                    RectTransform rect = obj.GetComponent<RectTransform>();
                    float x = currentStartX + col * spacing + layerOffsetX;
                    float y = currentStartY - row * spacing + layerOffsetY;
                    rect.anchoredPosition = new Vector2(x, y);

                    obj.transform.SetSiblingIndex(tileParent.childCount);

                    index++;
                }
            }
        }
    }

    public void ClearBoard()
    {
        List<GameObject> children = new List<GameObject>();

        foreach (Transform child in tileParent)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            Destroy(child);
        }
    }

    public Transform GetTileParent()
    {
        return tileParent;
    }
}