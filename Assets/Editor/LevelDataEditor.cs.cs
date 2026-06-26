using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(20);

        GUILayout.Label(
            "Level Preview",
            EditorStyles.boldLabel
        );

        LevelData level = (LevelData)target;

        float cellSize = 22f;

        Color[] layerColors =
        {
            new Color(0.4f, 1f, 0.4f),
            new Color(0.4f, 0.8f, 1f),
            new Color(1f, 0.6f, 0.6f),
            new Color(1f, 1f, 0.5f),
            new Color(1f, 0.5f, 1f)
        };

        Rect previewRect = GUILayoutUtility.GetRect(400, 400);

        int maxLayers = level.layers.Count;
        if (maxLayers == 0) return;
        
        int topLayerIndex = maxLayers - 1;

        float previewScale = cellSize / 130f;
        float previewOffsetX = level.stackOffsetX * previewScale;
        float previewOffsetY = level.stackOffsetY * previewScale;

        float centerX = previewRect.x + (previewRect.width / 2f);
        float centerY = previewRect.y + (previewRect.height / 2f);

        for (int layer = 0; layer < maxLayers; layer++)
        {
            ShapeData shape = level.layers[layer];

            if (shape == null || shape.layout == null || shape.layout.Length == 0)
                continue;

            string[] layout = shape.layout;

            Color color = layerColors[layer % layerColors.Length];

            int depthFromTop = topLayerIndex - layer;
            float currentLayerOffsetX = 0f;
            float currentLayerOffsetY = 0f;

            // ---> NEW: Match the BoardSpawner math with Cascade <---
            if (level.stackStyle == StackStyle.ZigZag)
            {
                if (depthFromTop == 0) currentLayerOffsetX = 0f;
                else if (depthFromTop % 2 == 1) currentLayerOffsetX = -previewOffsetX;
                else currentLayerOffsetX = previewOffsetX;
                
                currentLayerOffsetY = depthFromTop * previewOffsetY; 
            }
            else if (level.stackStyle == StackStyle.Cascade)
            {
                currentLayerOffsetX = depthFromTop * previewOffsetX;
                currentLayerOffsetY = depthFromTop * previewOffsetY;
            }
            else
            {
                currentLayerOffsetX = depthFromTop * -previewOffsetX;
                currentLayerOffsetY = depthFromTop * previewOffsetY; 
            }

            float currentWidth = (layout[0].Length - 1) * cellSize;
            float currentHeight = (layout.Length - 1) * cellSize;
            
            float currentStartX = centerX - (currentWidth / 2f);
            float currentStartY = centerY - (currentHeight / 2f);

            for (int row = 0; row < layout.Length; row++)
            {
                for (int col = 0; col < layout[row].Length; col++)
                {
                    if (layout[row][col] == '1')
                    {
                        float x = currentStartX + (col * cellSize) + currentLayerOffsetX;
                        float y = currentStartY + (row * cellSize) + currentLayerOffsetY;

                        float drawX = x - (cellSize / 2f);
                        float drawY = y - (cellSize / 2f);

                        EditorGUI.DrawRect(new Rect(drawX, drawY, cellSize - 2, cellSize - 2), color);
                    }
                }
            }
        }
    }
}