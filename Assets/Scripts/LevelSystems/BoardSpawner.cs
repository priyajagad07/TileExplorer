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
    private Sequence spawnSequence;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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

        List<GameObject> spawnedThisGeneration = new List<GameObject>();

        int index = 0;
        float spacingX = 145f;
        float spacingY = 144f;

        float stackOffsetY = proceduralLevelData.stackOffsetY;
        float stackOffsetX = proceduralLevelData.stackOffsetX;
        StackStyle style = proceduralLevelData.stackStyle;

        int maxCols = proceduralLevelData.layout[0].Length;
        int maxRows = proceduralLevelData.layout.Length;
        int maxLayers = proceduralLevelData.layerLayouts.Count;

        float totalShapeWidth = maxCols * spacingX;
        if (style == StackStyle.ZigZag) totalShapeWidth += (Mathf.Abs(stackOffsetX) * 2f);
        else totalShapeWidth += Mathf.Abs((maxLayers - 1) * stackOffsetX);

        float totalShapeHeight = (maxRows * spacingY) + Mathf.Abs((maxLayers - 1) * stackOffsetY);

        Canvas.ForceUpdateCanvases();

        float fallbackWidth = 1080f;
        float fallbackHeight = 1920f * 0.6f;

        // Grab the CanvasScaler to know the exact target resolution of your game
        UnityEngine.UI.CanvasScaler scaler = tileParent.GetComponentInParent<UnityEngine.UI.CanvasScaler>();
        if (scaler != null)
        {
            fallbackWidth = scaler.referenceResolution.x;
            fallbackHeight = scaler.referenceResolution.y * 0.6f;
        }

        // If the screen is hidden (width < 10), it uses the guaranteed fallback resolution
        float rawWidth = tileParent.rect.width > 10f ? tileParent.rect.width : fallbackWidth;
        float rawHeight = tileParent.rect.height > 10f ? tileParent.rect.height : fallbackHeight;

        // ---------------------------------------------------------

        float availableWidth = rawWidth - 100f;
        float availableHeight = rawHeight - 100f;

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

            float currentWidth = (currentCols - 1) * spacingX;
            float currentHeight = (currentRows - 1) * spacingY;

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

                    GameObject obj = ObjectPoolManager.Instance.Spawn(tiles[index], tileParent);
                    spawnedThisGeneration.Add(obj);

                    Tile tileScript = obj.GetComponent<Tile>();
                    tileScript.ResetTileState();

                    tileScript.row = row;
                    tileScript.col = col;
                    tileScript.layer = layer;

                    RectTransform rect1 = obj.GetComponent<RectTransform>();

                    float x = currentStartX + col * spacingX + layerOffsetX;
                    float y = currentStartY - row * spacingY + layerOffsetY;

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

            foreach (Transform child in tileParent)
            {
                if (!child.gameObject.activeSelf) continue;

                RectTransform rect = child.GetComponent<RectTransform>();
                Vector2 pos = rect.anchoredPosition;

                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y < minY) minY = pos.y;
                if (pos.y > maxY) maxY = pos.y;
            }

            Vector2 boundingBoxCenter = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);

            foreach (Transform child in tileParent)
            {
                if (!child.gameObject.activeSelf) continue;

                RectTransform rect = child.GetComponent<RectTransform>();
                Vector2 newPos = rect.anchoredPosition - boundingBoxCenter;
                rect.anchoredPosition3D = new Vector3(newPos.x, newPos.y, 0f);
            }
        }

        int currentLevelIndex = 0;
        if (SaveManager.instance != null && SaveManager.instance.data != null)
        {
            currentLevelIndex = SaveManager.instance.data.level;
        }

        if (currentLevelIndex >= 5)
        {
            Tile[] allSpawnedTiles = tileParent.GetComponentsInChildren<Tile>(false); // 'false' ignores inactive objects

            if (allSpawnedTiles.Length >= 3)
            {
                int amountOfJellies = Random.Range(2, 4);
                List<Tile> availableTilesForJelly = new List<Tile>(allSpawnedTiles);

                for (int j = 0; j < amountOfJellies; j++)
                {
                    if (availableTilesForJelly.Count == 0) break;

                    int randomIndex = Random.Range(0, availableTilesForJelly.Count);
                    Tile randomTile = availableTilesForJelly[randomIndex];

                    int randomClicks = Random.Range(5, 11);
                    randomTile.MakeJelly(randomClicks);
                    availableTilesForJelly.RemoveAt(randomIndex);
                }
            }
        }

        foreach (Tile tile in tileParent.GetComponentsInChildren<Tile>(false))
        {
            tile.CacheSpawnSize();
        }

        HashSet<GameObject> uniqueSpawned =
    new HashSet<GameObject>(spawnedThisGeneration);

        Debug.Log(
            $"========== SPAWN VALIDATION ==========\n" +
            $"Requested Tiles: {tiles.Count}\n" +
            $"Spawned Calls: {spawnedThisGeneration.Count}\n" +
            $"Unique GameObjects: {uniqueSpawned.Count}\n" +
            $"Active Children: {tileParent.GetComponentsInChildren<Tile>(false).Length}"
        );

        if (spawnedThisGeneration.Count != tiles.Count)
        {
            Debug.LogError(
                $"🚨 SPAWN COUNT MISMATCH! " +
                $"Requested {tiles.Count}, but only " +
                $"{spawnedThisGeneration.Count} spawn positions were filled."
            );
        }

        if (uniqueSpawned.Count != spawnedThisGeneration.Count)
        {
            Debug.LogError(
                "🚨 DUPLICATE POOLED GAMEOBJECT DETECTED!"
            );
        }
    }

    public void PlaySpawnAnimation(bool playSound = true)
    {
        if (isSpawning)
            return;

        spawnSequence?.Kill(false);
        spawnSequence = null;

        Tile[] allTiles =
            tileParent.GetComponentsInChildren<Tile>(false);

        if (allTiles.Length == 0)
        {
            Debug.LogWarning(
                "BoardSpawner: No active tiles found for spawn animation."
            );

            isSpawning = false;
            return;
        }

        isSpawning = true;
        int highestLayer = GetHighestLayer(allTiles);

        spawnSequence = DOTween.Sequence();
        Sequence masterSequence = spawnSequence;
        float currentTime = 0f;
        float dropDuration = 0.2f;
        float delayBetweenTiles = 0.015f;
        float pauseBetweenLayers = 0.015f;
        float dropHeight = 3500f;

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
            spawnSequence = null;
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
        spawnSequence?.Kill(false);
        spawnSequence = null;

        isSpawning = false;

        List<GameObject> children = new List<GameObject>();

        foreach (Transform child in tileParent)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            if (child == null)
                continue;

            child.transform.DOKill();

            ObjectPoolManager.Instance.Despawn(child);
        }

        Debug.Log(
            $"BOARD CLEARED: {children.Count} tiles despawned."
        );
    }

    public Transform GetTileParent()
    {
        return tileParent;
    }
}