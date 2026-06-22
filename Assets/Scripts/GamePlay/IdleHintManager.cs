using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class IdleHintManager : MonoBehaviour
{
    public static IdleHintManager instance;

    [Header("Settings")]
    public float timeBeforeHint = 10f;

    private float idleTimer = 0f;
    private bool isHinting = false;
    private List<Tile> currentlyHintedTiles = new List<Tile>();
    private Dictionary<Tile, int> originalSiblingIndices = new Dictionary<Tile, int>();

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (MatchBoard.instance == null || MatchBoard.instance.isInputLocked) return;
        if (TutorialManager.instance != null && TutorialManager.instance.isTutorialActive) return;

        if (!isHinting)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= timeBeforeHint)
            {
                ShowIdleHint();
            }
        }
    }

    public void ResetIdleTimer()
    {
        idleTimer = 0f;

        if (isHinting)
        {
            StopHints();
        }
    }

    void ShowIdleHint()
    {
        if (BoardSpawner.instance == null) return;
        Transform tileParent = BoardSpawner.instance.GetTileParent();

        // 1. Check what is currently in the tray
        List<GameObject> placedTiles = MatchBoard.instance.GetPlacedTiles();
        Dictionary<int, int> trayCounts = new Dictionary<int, int>();
        foreach (GameObject pTile in placedTiles)
        {
            if (pTile == null) continue;
            Tile tile = pTile.GetComponent<Tile>();
            if (tile != null && !tile.isMatched)
            {
                if (!trayCounts.ContainsKey(tile.tileId)) trayCounts[tile.tileId] = 0;
                trayCounts[tile.tileId]++;
            }
        }

        // 2. Find all unblocked tiles on the board
        List<Tile> unblockedTiles = new List<Tile>();
        foreach (Transform child in tileParent)
        {
            if (child == null) continue;
            Tile tile = child.GetComponent<Tile>();
            if (tile == null || tile.IsMoved() || tile.IsBlocked()) continue;

            unblockedTiles.Add(tile);
        }

        int targetTileId = -1;

        // PRIORITY 1: Help them finish a match they already started in the tray
        foreach (var kvp in trayCounts)
        {
            int neededToMatch = 3 - kvp.Value;
            int availableOnBoard = 0;

            foreach (Tile t in unblockedTiles)
            {
                if (t.tileId == kvp.Key) availableOnBoard++;
            }

            if (availableOnBoard >= neededToMatch)
            {
                targetTileId = kvp.Key;
                break;
            }
        }

        // PRIORITY 2: If the tray has no matches, show them ANY 3 matching tiles on the board
        if (targetTileId == -1)
        {
            Dictionary<int, int> boardCounts = new Dictionary<int, int>();
            foreach (Tile t in unblockedTiles)
            {
                if (!boardCounts.ContainsKey(t.tileId)) boardCounts[t.tileId] = 0;
                boardCounts[t.tileId]++;
            }

            foreach (var kvp in boardCounts)
            {
                if (kvp.Value >= 3)
                {
                    targetTileId = kvp.Key;
                    break;
                }
            }
        }

        // 3. Start the glowing animation!
        if (targetTileId != -1)
        {
            isHinting = true;
            int highlighted = 0;
            int needed = trayCounts.ContainsKey(targetTileId) ? 3 - trayCounts[targetTileId] : 3;

            foreach (Tile t in unblockedTiles)
            {
                if (t.tileId == targetTileId && highlighted < needed)
                {
                    currentlyHintedTiles.Add(t);

                    originalSiblingIndices[t] = t.transform.GetSiblingIndex();
                    t.transform.SetAsLastSibling();

                    AnimateHint(t.transform);
                    highlighted++;
                }
            }
        }
        else
        {
            idleTimer = 0f;
        }
    }

    void AnimateHint(Transform tileTransform)
    {
        tileTransform.DOKill();

        tileTransform.DOScale(Vector3.one * 1.15f, 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void StopHints()
    {
        isHinting = false;

        foreach (Tile t in currentlyHintedTiles)
        {
            if (t != null)
            {
                t.transform.DOKill();
                t.transform.localScale = Vector3.one;

                if (originalSiblingIndices.ContainsKey(t))
                {
                    t.transform.SetSiblingIndex(originalSiblingIndices[t]);
                }
            }
        }

        currentlyHintedTiles.Clear();
        originalSiblingIndices.Clear();
    }
}