using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Solo.MOST_IN_ONE;

public class BoosterSystem : MonoBehaviour
{
    public static BoosterSystem instance;
    private Stack<UndoData> undoStack = new Stack<UndoData>();

    void Awake()
    {
        instance = this;
    }

    public void RecordMove(GameObject tile)
    {
        RectTransform rect = tile.GetComponent<RectTransform>();
        UndoData data = new UndoData(
            tile,
            tile.transform.parent,
            rect.anchoredPosition,
            tile.transform.GetSiblingIndex()
        );

        undoStack.Push(data);
    }

    public bool UndoMove()
    {
        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.MediumImpact);

        while (undoStack.Count > 0)
        {
            UndoData data = undoStack.Pop();

            if (data.tile == null)
                continue;

            MatchBoard.instance.RemoveTile(data.tile);

            data.tile.transform.SetParent(data.originalParent);
            data.tile.transform.SetSiblingIndex(data.siblingIndex);

            RectTransform rect = data.tile.GetComponent<RectTransform>();

            rect.anchoredPosition = data.originalPosition;

            UndoBounce(data.tile.transform);

            Tile tileScript = data.tile.GetComponent<Tile>();
            tileScript.SetMoved(false);

            Tile.RefreshAllTileVisuals(data.originalParent);

            MatchBoard.instance.RearrangeBoard();

            SoundManager.instance.PlaySound(SoundName.TileMoveToBoard);

            return true;
        }

        return false;
    }

    void UndoBounce(Transform tile)
    {
        tile.DOKill();

        Vector3 originalScale =
            tile.localScale;

        tile.localScale =
            originalScale * 1.25f;

        tile.DOScale(
            originalScale,
            0.25f
        )
        .SetEase(Ease.OutBack);
    }

    public void ClearUndoStack()
    {
        undoStack.Clear();
    }

    public bool CanUndo()
    {
        foreach (UndoData data in undoStack)
        {
            if (data.tile != null)
            {
                return true;
            }
        }

        return false;
    }
    public void ShuffleTiles()
    {
        int completedAnimations = 0;
        int totalAnimations = 0;

        if (BoardSpawner.instance == null)
        {
            Debug.Log("BoardSpawner Missing");
            return;
        }

        Transform tileParent = BoardSpawner.instance.GetTileParent();
        if (tileParent == null)
        {
            Debug.LogError("Tile Parent is NULL");
            return;
        }

        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.HeavyImpact);

        Dictionary<int, List<Tile>> layerGroups = new Dictionary<int, List<Tile>>();

        foreach (Transform child in tileParent)
        {
            if (child == null)
                continue;

            Tile tile = child.GetComponent<Tile>();

            if (tile == null)
                continue;

            if (tile.IsMoved())
                continue;

            if (!layerGroups.ContainsKey(tile.layer))
            {
                layerGroups[tile.layer] = new List<Tile>();
            }

            layerGroups[tile.layer].Add(tile);
        }

        foreach (var group in layerGroups)
        {
            List<Tile> tiles = group.Value;

            List<Vector2> positions = new List<Vector2>();

            foreach (Tile tile in tiles)
            {
                positions.Add(tile.GetComponent<RectTransform>().anchoredPosition);
            }

            for (int i = 0; i < positions.Count; i++)
            {
                int randomIndex = Random.Range(i, positions.Count);

                Vector2 tempPos = positions[i];
                positions[i] = positions[randomIndex];
                positions[randomIndex] = tempPos;
            }

            totalAnimations += tiles.Count;

            for (int i = 0; i < tiles.Count; i++)
            {
                RectTransform rect = tiles[i].GetComponent<RectTransform>();
                AnimateShuffle(rect, positions[i], () =>
                            {
                                completedAnimations++;

                                if (completedAnimations >= totalAnimations)
                                {
                                    Tile.RefreshAllTileVisuals(tileParent);
                                }
                            });
            }
        }

        SoundManager.instance.PlaySound(SoundName.TileMoveToBoard);
    }

    void AnimateShuffle(RectTransform rect, Vector2 targetPos, System.Action onComplete = null)
    {
        rect.DOKill();

        float targetRotation = Random.Range(-30f, 30f);

        Sequence seq = DOTween.Sequence();

        seq.Append(rect.DOAnchorPos(targetPos, 0.35f).SetEase(Ease.OutCubic));

        seq.Join(rect.DORotate(new Vector3(0, 0, targetRotation), 0.2f));

        seq.Append(rect.DORotate(Vector3.zero, 0.15f));

        seq.OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    public void UseMagicBooster()
    {
        if (BoardSpawner.instance == null)
        {
            Debug.Log("BoardSpawner Missing");
            return;
        }

        Transform tileParent = BoardSpawner.instance.GetTileParent();

        if (tileParent == null)
            return;

        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.HeavyImpact);

        List<GameObject> placedTiles = MatchBoard.instance.GetPlacedTiles();
        Dictionary<int, int> placedCounts = new Dictionary<int, int>();

        foreach (GameObject pTile in placedTiles)
        {
            if (pTile == null)
                continue;

            Tile tile = pTile.GetComponent<Tile>();

            if (tile != null)
            {
                if (!placedCounts.ContainsKey(tile.tileId))
                {
                    placedCounts[tile.tileId] = 0;
                }
                placedCounts[tile.tileId]++;
            }
        }

        int targetTileId = -1;
        int neededToMatch = 3;

        foreach (var kvp in placedCounts)
        {
            if (kvp.Value > 0 && kvp.Value < 3)
            {
                targetTileId = kvp.Key;
                neededToMatch = 3 - kvp.Value;
                break;
            }
        }

        List<Tile> availableBoardTiles = new List<Tile>();
        foreach (Transform child in tileParent)
        {
            if (child == null)
                continue;

            Tile tile = child.GetComponent<Tile>();

            if (tile == null || tile.IsMoved())
                continue;

            availableBoardTiles.Add(tile);
        }

        if (targetTileId != -1)
        {
            int found = 0;
            foreach (Tile tile in availableBoardTiles)
            {
                if (tile.tileId == targetTileId)
                {
                    MagicMove(tile);
                    found++;

                    if (found == neededToMatch)
                    {
                        SoundManager.instance.PlaySound(SoundName.ThreeTilesMatch);
                        return;
                    }
                }
            }
        }

        Dictionary<int, List<Tile>> tileGroups = new Dictionary<int, List<Tile>>();
        foreach (Tile tile in availableBoardTiles)
        {
            if (!tileGroups.ContainsKey(tile.tileId)) tileGroups[tile.tileId] = new List<Tile>();
            tileGroups[tile.tileId].Add(tile);
        }

        foreach (var group in tileGroups)
        {
            if (group.Value.Count >= 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    MagicMove(group.Value[i]);
                }

                SoundManager.instance.PlaySound(SoundName.ThreeTilesMatch);
                return;
            }
        }

        Debug.Log("No valid magic match possible on the board");
    }

    void MagicMove(Tile tile)
    {
        Transform t = tile.transform;

        t.DOKill();

        Vector3 originalScale = t.localScale;

        Sequence seq = DOTween.Sequence();

        seq.Append(t.DOScale(originalScale * 1.2f, 0.12f));

        seq.Append(t.DOScale(originalScale, 0.12f));

        seq.OnComplete(() =>
        {
            tile.MoveToBoard();
        });
    }
}