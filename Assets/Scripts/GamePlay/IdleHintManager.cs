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

    private readonly List<Tile> currentlyHintedTiles = new List<Tile>();
    private readonly Dictionary<Tile, int> originalSiblingIndices = new Dictionary<Tile, int>();
    private readonly Dictionary<Tile, Vector3> originalScales = new Dictionary<Tile, Vector3>();

    // Reused buffers to avoid per-hint allocations (cleared before each use).
    private readonly Dictionary<int, int> trayCounts = new Dictionary<int, int>();
    private readonly List<Tile> unblockedTiles = new List<Tile>();
    private readonly List<int> sortedTrayIds = new List<int>();
    private readonly Dictionary<int, int> boardCounts = new Dictionary<int, int>();

    private System.Comparison<int> trayCountDescendingComparer;

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

        // Cache once so Sort does not allocate a new delegate each hint.
        trayCountDescendingComparer = CompareTrayCountsDescending;
    }

    void Update()
    {
        if (BoardSpawner.instance == null)
        {
            ResetIdleTimer();
            return;
        }

        Transform tileParent = BoardSpawner.instance.GetTileParent();
        if (tileParent == null || tileParent.childCount == 0)
        {
            ResetIdleTimer();
            return;
        }

        if (MatchBoard.instance == null ||
            MatchBoard.instance.isInputLocked)
        {
            ResetIdleTimer();
            return;
        }

        // Block idle hints while ANY tutorial is active
        // (Level 1 hard tutorial, Shuffle/Magic soft tutorials,
        // or Undo soft tutorial). Never show two hint cursors.
        if (IsAnyTutorialActive())
        {
            ResetIdleTimer();
            return;
        }

        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            ResetIdleTimer();
        }

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

    private static bool IsAnyTutorialActive()
    {
        if (TutorialManager.instance != null &&
            TutorialManager.instance.IsAnyTutorialActive)
        {
            return true;
        }

        if (UndoTutorialManager.instance != null &&
            UndoTutorialManager.instance.IsRunning)
        {
            return true;
        }

        return false;
    }

    void ShowIdleHint()
    {
        if (IsAnyTutorialActive())
        {
            ResetIdleTimer();
            return;
        }

        if (BoardSpawner.instance == null || MatchBoard.instance == null)
            return;

        Transform tileParent = BoardSpawner.instance.GetTileParent();
        if (tileParent == null)
            return;

        List<GameObject> placedTiles = MatchBoard.instance.GetPlacedTiles();

        trayCounts.Clear();
        for (int i = 0; i < placedTiles.Count; i++)
        {
            GameObject pTile = placedTiles[i];
            if (pTile == null)
                continue;

            Tile tile = pTile.GetComponent<Tile>();

            if (tile != null && !tile.isMatched)
            {
                if (trayCounts.TryGetValue(tile.tileId, out int count))
                {
                    trayCounts[tile.tileId] = count + 1;
                }
                else
                {
                    trayCounts[tile.tileId] = 1;
                }
            }
        }

        unblockedTiles.Clear();
        foreach (Transform child in tileParent)
        {
            if (child == null)
                continue;
            if (!child.gameObject.activeSelf) continue;

            Tile tile = child.GetComponent<Tile>();

            if (tile == null ||
                tile.IsMoved() ||
                tile.IsBlocked() ||
                tile.isJellyLocked)
                continue;

            unblockedTiles.Add(tile);
        }

        int targetTileId = -1;

        sortedTrayIds.Clear();
        foreach (int id in trayCounts.Keys)
        {
            sortedTrayIds.Add(id);
        }

        sortedTrayIds.Sort(trayCountDescendingComparer);

        for (int i = 0; i < sortedTrayIds.Count; i++)
        {
            int id = sortedTrayIds[i];
            int availableOnBoard = 0;

            for (int t = 0; t < unblockedTiles.Count; t++)
            {
                if (unblockedTiles[t].tileId == id)
                {
                    availableOnBoard++;
                }
            }

            if (availableOnBoard > 0)
            {
                targetTileId = id;
                break;
            }
        }

        if (targetTileId == -1)
        {
            boardCounts.Clear();

            for (int t = 0; t < unblockedTiles.Count; t++)
            {
                int id = unblockedTiles[t].tileId;
                if (boardCounts.TryGetValue(id, out int count))
                {
                    boardCounts[id] = count + 1;
                }
                else
                {
                    boardCounts[id] = 1;
                }
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

        if (targetTileId == -1)
        {
            idleTimer = 0f;
            return;
        }

        isHinting = true;

        currentlyHintedTiles.Clear();
        originalSiblingIndices.Clear();
        originalScales.Clear();

        int highlighted = 0;

        int needed = trayCounts.TryGetValue(targetTileId, out int trayCount)
            ? 3 - trayCount
            : 3;

        for (int t = 0; t < unblockedTiles.Count; t++)
        {
            Tile tile = unblockedTiles[t];
            if (tile.tileId == targetTileId && highlighted < needed)
            {
                currentlyHintedTiles.Add(tile);
                originalSiblingIndices[tile] = tile.transform.GetSiblingIndex();
                originalScales[tile] = tile.transform.localScale;
                tile.transform.SetAsLastSibling();
                AnimateHint(tile.transform);
                highlighted++;
            }
        }
    }

    private int CompareTrayCountsDescending(int a, int b)
    {
        return trayCounts[b].CompareTo(trayCounts[a]);
    }

    void AnimateHint(Transform tileTransform)
    {
        string tweenId = tileTransform.GetInstanceID() + "_hintScale";
        DOTween.Kill(tweenId);
        tileTransform.DOScale(tileTransform.localScale * 1.15f, 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetId(tweenId);
    }

    public void StopHints()
    {
        isHinting = false;

        for (int i = 0; i < currentlyHintedTiles.Count; i++)
        {
            Tile t = currentlyHintedTiles[i];
            if (t == null || t.transform == null)
                continue;

            string tweenId =
                t.transform.GetInstanceID() + "_hintScale";

            DOTween.Kill(tweenId);

            if (originalScales.TryGetValue(t, out Vector3 originalScale))
            {
                t.transform.localScale = originalScale;
            }

            if (originalSiblingIndices.TryGetValue(t, out int siblingIndex))
            {
                t.transform.SetSiblingIndex(siblingIndex);
            }
        }

        currentlyHintedTiles.Clear();
        originalSiblingIndices.Clear();
        originalScales.Clear();
    }
}
