using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Solo.MOST_IN_ONE;

public class BoosterSystem : MonoBehaviour
{
    public static BoosterSystem instance;
    private Stack<UndoData> undoStack = new Stack<UndoData>();
    public bool justShuffled = false;
    private MatchBoardMovement movement;

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

    public UndoData CreateUndoData(GameObject tile)
    {
        if (tile == null)
            return null;

        RectTransform rect =
            tile.GetComponent<RectTransform>();

        Tile tileScript =
            tile.GetComponent<Tile>();

        if (rect == null || tileScript == null)
            return null;

        return new UndoData(
            tile,
            tile.transform.parent,
            rect.anchoredPosition,
            tile.transform.GetSiblingIndex(),
            rect.localScale,
            tileScript.GetCurrentSize(),
            tileScript.GetCurrentImageSize()
        );
    }

    public void CommitUndoData(UndoData data)
    {
        if (data == null)
            return;

        justShuffled = false;
        undoStack.Push(data);

        Debug.Log(
            $"UNDO RECORDED: {data.tile.name} | " +
            $"Scale: {data.originalScale} | " +
            $"Size: {data.originalSizeDelta} | " +
            $"Image Size: {data.originalImageSizeDelta}"
        );
    }

    public bool UndoMove()
    {
        Debug.Log(
            "Undo Stack Count = " +
            undoStack.Count
        );

        SoundManager.instance.PlayHaptic(
            MOST_HapticFeedback.HapticTypes.MediumImpact
        );

        while (undoStack.Count > 0)
        {
            UndoData data =
                undoStack.Pop();

            if (data.tile == null)
            {
                Debug.Log(
                    "Tile destroyed, skipping."
                );

                continue;
            }

            if (!MatchBoard.instance
                .GetPlacedTiles()
                .Contains(data.tile))
            {
                Debug.Log(
                    "Tile already removed or matched, skipping."
                );

                continue;
            }

            Debug.Log(
                "Undoing tile = " +
                data.tile.name
            );

            // Remove it from the match tray first.
            MatchBoard.instance.RemoveTile(
                data.tile
            );

            // Important: kill every active tween left
            // from the tray movement/scale animation.
            data.tile.transform.DOKill();

            RectTransform rect =
                data.tile.GetComponent<RectTransform>();

            rect.DOKill();

            // Return to the original board parent.
            data.tile.transform.SetParent(
                data.originalParent,
                false
            );

            // Restore original hierarchy order.
            data.tile.transform.SetSiblingIndex(
                data.siblingIndex
            );

            // Restore exact board position.
            rect.anchoredPosition =
                data.originalPosition;

            Tile tileScript =
                data.tile.GetComponent<Tile>();

            // It is now a main-board tile again.
            tileScript.SetMoved(false);

            // Restore the exact size and scale
            // captured before it entered the tray.
            tileScript.RestoreExactBoardSize(
                data.originalScale,
                data.originalSizeDelta,
                data.originalImageSizeDelta
            );

            // The movement system must treat this tile
            // as a new tray arrival if clicked again.
            MatchBoard.instance.ForgetTrayTile(
                data.tile
            );

            // Refresh blocked/unblocked visuals.
            Tile.RefreshAllTileVisuals(
                data.originalParent
            );

            // Rearrange remaining tray tiles.
            MatchBoard.instance.RearrangeBoard();

            if (AutoShuffleManager.instance != null)
            {
                AutoShuffleManager.instance
                    .CheckForDeadlock();
            }

            SoundManager.instance.PlaySound(
                SoundName.TileMoveToBoard
            );

            Debug.Log(
                $"UNDO RESTORED: {data.tile.name} | " +
                $"Scale: {rect.localScale} | " +
                $"Size: {rect.sizeDelta}"
            );

            return true;
        }

        return false;
    }

    public void ClearUndoStack()
    {
        undoStack.Clear();
    }

    public bool CanUndo()
    {
        if (MatchBoard.instance == null)
            return false;

        List<GameObject> placedTiles =
            MatchBoard.instance.GetPlacedTiles();

        foreach (UndoData data in undoStack)
        {
            if (data != null &&
                data.tile != null &&
                placedTiles.Contains(data.tile))
            {
                Tile tile =
                    data.tile.GetComponent<Tile>();

                if (tile != null &&
                    tile.IsMoved() &&
                    !tile.isMatched)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void ShuffleTiles()
    {
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

        MatchBoard.instance.isInputLocked = true;
        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.HeavyImpact);

        ClearUndoStack();

        justShuffled = true;

        List<Tile> activeTiles = new List<Tile>();
        List<Vector2> originalPositions = new List<Vector2>();
        List<int> originalLayers = new List<int>();
        List<int> originalSiblingIndices = new List<int>();

        foreach (Transform child in tileParent)
        {
            if (child == null) continue;

            Tile tile = child.GetComponent<Tile>();
            if (tile == null || tile.IsMoved()) continue;

            activeTiles.Add(tile);
            originalPositions.Add(child.GetComponent<RectTransform>().anchoredPosition);
            originalLayers.Add(tile.layer);
            originalSiblingIndices.Add(child.GetSiblingIndex());
        }

        if (activeTiles.Count <= 1)
        {
            MatchBoard.instance.isInputLocked = false;
            return;
        }

        List<Tile> shuffledTiles = new List<Tile>(activeTiles);
        for (int i = 0; i < shuffledTiles.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledTiles.Count);
            Tile temp = shuffledTiles[i];
            shuffledTiles[i] = shuffledTiles[randomIndex];
            shuffledTiles[randomIndex] = temp;
        }

        int completedAnimations = 0;
        int totalAnimations = activeTiles.Count;

        Dictionary<Tile, int> tileToTargetSibling = new Dictionary<Tile, int>();
        Dictionary<Tile, Vector2> tileToTargetPos = new Dictionary<Tile, Vector2>();

        for (int i = 0; i < shuffledTiles.Count; i++)
        {
            Tile tile = shuffledTiles[i];
            tile.layer = originalLayers[i];
            tileToTargetPos[tile] = originalPositions[i];
            tileToTargetSibling[tile] = originalSiblingIndices[i];
        }

        shuffledTiles.Sort((a, b) => tileToTargetSibling[a].CompareTo(tileToTargetSibling[b]));
        foreach (Tile t in shuffledTiles)
        {
            t.transform.SetSiblingIndex(tileToTargetSibling[t]);
        }

        foreach (Tile t in shuffledTiles)
        {
            RectTransform rect = t.GetComponent<RectTransform>();
            Vector2 targetPos = tileToTargetPos[t];

            AnimateShuffle(rect, targetPos, () =>
            {
                completedAnimations++;

                if (completedAnimations >= totalAnimations)
                {
                    Tile.RefreshAllTileVisuals(tileParent);
                    MatchBoard.instance.isInputLocked = false;
                }
            });
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

    public bool CanShuffle()
    {
        if (BoardSpawner.instance == null) return false;
        Transform tileParent = BoardSpawner.instance.GetTileParent();

        int availableTiles = 0;
        foreach (Transform child in tileParent)
        {
            Tile tile = child.GetComponent<Tile>();
            if (tile != null && !tile.IsMoved())
            {
                availableTiles++;
            }
        }

        return availableTiles > 1;
    }

    public bool CanUseMagic()
    {
        if (BoardSpawner.instance == null)
            return false;

        Transform tileParent =
            BoardSpawner.instance.GetTileParent();

        int availableTiles = 0;

        foreach (Transform child in tileParent)
        {
            Tile tile = child.GetComponent<Tile>();

            if (tile != null &&
                !tile.IsMoved() &&
                !tile.isJellyLocked)
            {
                availableTiles++;
            }
        }

        return availableTiles > 0;
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

            if (tile == null || tile.IsMoved() || tile.isJellyLocked)
            {
                continue;
            }

            availableBoardTiles.Add(tile);
        }

        availableBoardTiles.Sort((a, b) => b.layer.CompareTo(a.layer));

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