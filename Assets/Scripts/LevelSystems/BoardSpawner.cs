using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

public class BoardSpawner : MonoBehaviour
{
    public static BoardSpawner instance;
    public bool isSpawning = false;
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
        float spacing = 130f;

        float stackOffsetY = proceduralLevelData.stackOffsetY;
        float stackOffsetX = proceduralLevelData.stackOffsetX;
        StackStyle style = proceduralLevelData.stackStyle;

        int maxCols = proceduralLevelData.layout[0].Length;
        int maxRows = proceduralLevelData.layout.Length;
        int maxLayers = proceduralLevelData.layerLayouts.Count;

        float totalShapeWidth = maxCols * spacing;
        if (style == StackStyle.ZigZag) totalShapeWidth += (Mathf.Abs(stackOffsetX) * 2f);
        else totalShapeWidth += Mathf.Abs((maxLayers - 1) * stackOffsetX);

        float totalShapeHeight = (maxRows * spacing) + Mathf.Abs((maxLayers - 1) * stackOffsetY);

        Canvas.ForceUpdateCanvases();

        float rawWidth = tileParent.rect.width > 0 ? tileParent.rect.width : Screen.width;
        float rawHeight = tileParent.rect.height > 0 ? tileParent.rect.height : Screen.height * 0.6f;

        float availableWidth = rawWidth - 50f;
        float availableHeight = rawHeight - 50f;

        float scaleX = availableWidth / totalShapeWidth;
        float scaleY = availableHeight / totalShapeHeight;

        float tileScale = Mathf.Min(scaleX, scaleY);
        if (tileScale > 1f) tileScale = 1f;

        tileParent.localScale = Vector3.one * tileScale;

        int topLayerIndex = maxLayers - 1;

        for (int layer = 0; layer < maxLayers; layer++)
        {
            string[] currentLayout = proceduralLevelData.layerLayouts[layer];

            int currentRows = currentLayout.Length;
            int currentCols = currentLayout[0].Length;

            float currentWidth = (currentCols - 1) * spacing;
            float currentHeight = (currentRows - 1) * spacing;

            float currentStartX = -currentWidth / 2f;
            float currentStartY = currentHeight / 2f;

            float layerOffsetX = 0f;
            float layerOffsetY = 0f;

            int depthFromTop = topLayerIndex - layer;

            if (style == StackStyle.ZigZag)
            {
                if (depthFromTop == 0) layerOffsetX = 0f;
                else if (depthFromTop % 2 == 1) layerOffsetX = -stackOffsetX;
                else layerOffsetX = stackOffsetX;

                layerOffsetY = depthFromTop * -stackOffsetY;
            }
            else if (style == StackStyle.Cascade)
            {
                layerOffsetX = depthFromTop * stackOffsetX;
                layerOffsetY = depthFromTop * -stackOffsetY;
            }
            else
            {
                layerOffsetX = depthFromTop * -stackOffsetX;
                layerOffsetY = depthFromTop * -stackOffsetY;
            }

            for (int row = 0; row < currentRows; row++)
            {
                string rowData = currentLayout[row];

                for (int col = 0; col < currentCols; col++)
                {
                    if (index >= tiles.Count) break;
                    if (string.IsNullOrEmpty(rowData) || col >= rowData.Length) continue;
                    if (rowData[col] != '1') continue;

                    GameObject obj = Instantiate(tiles[index], tileParent);

                    Tile tileScript = obj.GetComponent<Tile>();
                    tileScript.row = row;
                    tileScript.col = col;
                    tileScript.layer = layer;

                    RectTransform rect1 = obj.GetComponent<RectTransform>();

                    float x = currentStartX + col * spacing + layerOffsetX;
                    float y = currentStartY - row * spacing + layerOffsetY;

                    rect1.anchoredPosition = new Vector2(x, y);

                    obj.transform.SetSiblingIndex(tileParent.childCount);

                    index++;
                }
            }
        }

        if (tileParent.childCount > 0)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            for (int i = 0; i < tileParent.childCount; i++)
            {
                RectTransform rect = tileParent.GetChild(i).GetComponent<RectTransform>();
                Vector2 pos = rect.anchoredPosition;

                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y < minY) minY = pos.y;
                if (pos.y > maxY) maxY = pos.y;
            }
            Vector2 boundingBoxCenter = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);

            for (int i = 0; i < tileParent.childCount; i++)
            {
                RectTransform rect = tileParent.GetChild(i).GetComponent<RectTransform>();
                rect.anchoredPosition -= boundingBoxCenter;
            }
        }
    }

    public void PlaySpawnAnimation(bool playSound = true)
    {
        if (isSpawning) return;
        isSpawning = true;

        Tile[] allTiles = tileParent.GetComponentsInChildren<Tile>();
        int highestLayer = GetHighestLayer(allTiles);

        Sequence masterSequence = DOTween.Sequence();
        float currentTime = 0f;
        float dropDuration = 0.2f;
        float delayBetweenTiles = 0.015f;
        float pauseBetweenLayers = 0.015f;
        float dropHeight = 2500f;

        bool glowSoundInserted = false;

        for (int layer = 0; layer <= highestLayer; layer++)
        {
            List<Tile> layerTiles = new List<Tile>();
            foreach (Tile tile in allTiles)
            {
                if (tile.layer == layer)
                {
                    layerTiles.Add(tile);
                }
            }

            if (layerTiles.Count == 0) continue;

            layerTiles = layerTiles
                .OrderByDescending(t => t.GetComponent<RectTransform>().anchoredPosition.y)
                .ThenBy(t => t.GetComponent<RectTransform>().anchoredPosition.x)
                .ToList();

            if (playSound)
            {
                masterSequence.InsertCallback(currentTime, () =>
            {
                SoundManager.instance.PlaySound(SoundName.TileSpawn);
            });
            }

            for (int i = 0; i < layerTiles.Count; i++)
            {
                Tile tile = layerTiles[i];
                RectTransform rect = tile.GetComponent<RectTransform>();

                rect.DOKill(true);

                Vector2 finalPos = rect.anchoredPosition;

                rect.localScale = Vector3.one * 0.9f;
                rect.anchoredPosition = finalPos + new Vector2(0, dropHeight);

                Sequence tileSeq = DOTween.Sequence();
                float startRotation = Random.Range(-16f, 16f);

                rect.localRotation = Quaternion.Euler(0, 0, startRotation);

                tileSeq.Append(
                    rect.DORotate(Vector3.zero, dropDuration)
                        .SetEase(Ease.OutCubic)
                );

                tileSeq.Join(rect.DOAnchorPos(finalPos, dropDuration).SetEase(Ease.OutBack));

                if (layer == highestLayer)
                {
                    if (!glowSoundInserted)
                    {
                        glowSoundInserted = true;

                        tileSeq.InsertCallback(dropDuration, () =>
                        {
                            if (playSound)
                            {
                                SoundManager.instance.PlaySound(SoundName.TileSpawnFinish);
                            }
                        });
                    }

                    Image[] images = tile.GetComponentsInChildren<Image>();

                    foreach (Image img in images)
                    {
                        Color original = img.color;

                        tileSeq.Insert(
                            dropDuration,
                            img.DOColor(new Color(1.15f, 1.15f, 0.9f, 1f), 0.1f)
                        );

                        tileSeq.Insert(
                            dropDuration + 0.1f,
                            img.DOColor(original, 0.2f)
                        );
                    }

                    tileSeq.Insert(
                        dropDuration,
                        rect.DOScale(1.12f, 0.08f)
                            .SetLoops(2, LoopType.Yoyo)
                    );
                }

                masterSequence.Insert(currentTime, tileSeq);
                currentTime += delayBetweenTiles;
            }

            currentTime += dropDuration + pauseBetweenLayers;
        }

        masterSequence.OnComplete(() =>
        {
            isSpawning = false;

            Tile.RefreshAllTileVisuals(tileParent);

            if (GameManager.instance != null)
            {
                GameManager.instance.StartGame();
            }

            if (TutorialManager.instance != null)
            {
                TutorialManager.instance.CheckAndStartTutorial();
            }

            if (BoosterManager.instance != null)
            {
                BoosterManager.instance.PlayUnlockAnimationIfNeeded();
            }

            if (IdleHintManager.instance != null)
            {
                IdleHintManager.instance.ResetIdleTimer();
            }
        });

        masterSequence.Play();
    }

    int GetHighestLayer(Tile[] tiles)
    {
        int highest = 0;
        foreach (Tile tile in tiles)
        {
            if (tile.layer > highest) highest = tile.layer;
        }
        return highest;
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
            child.transform.SetParent(null);
            Destroy(child);
        }
    }

    public Transform GetTileParent()
    {
        return tileParent;
    }
}