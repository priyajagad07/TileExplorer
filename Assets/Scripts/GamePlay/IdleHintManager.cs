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
    private Dictionary<Tile, Vector3> originalScales = new Dictionary<Tile, Vector3>();

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (BoardSpawner.instance == null ||
            BoardSpawner.instance.GetTileParent().childCount == 0)
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

        if (TutorialManager.instance != null &&
            TutorialManager.instance.isTutorialActive)
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

    void ShowIdleHint()
    {
        if (BoardSpawner.instance == null)
            return;

        Transform tileParent = BoardSpawner.instance.GetTileParent();
        List<GameObject> placedTiles = MatchBoard.instance.GetPlacedTiles();
        Dictionary<int, int> trayCounts = new Dictionary<int, int>();

        foreach (GameObject pTile in placedTiles)
        {
            if (pTile == null)
                continue;

            Tile tile = pTile.GetComponent<Tile>();

            if (tile != null && !tile.isMatched)
            {
                if (!trayCounts.ContainsKey(tile.tileId))
                {
                    trayCounts[tile.tileId] = 0;
                }

                trayCounts[tile.tileId]++;
            }
        }

        List<Tile> unblockedTiles = new List<Tile>();

        foreach (Transform child in tileParent)
        {
            if (child == null)
                continue;

            Tile tile = child.GetComponent<Tile>();

            if (tile == null ||
                tile.IsMoved() ||
                tile.IsBlocked())
                continue;

            unblockedTiles.Add(tile);
        }

        int targetTileId = -1;

        List<int> sortedTrayIds = new List<int>(trayCounts.Keys);
        sortedTrayIds.Sort((a, b) => trayCounts[b].CompareTo(trayCounts[a]));

        foreach (int id in sortedTrayIds)
        {
            int availableOnBoard = 0;

            foreach (Tile t in unblockedTiles)
            {
                if (t.tileId == id)
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
            Dictionary<int, int> boardCounts = new Dictionary<int, int>();

            foreach (Tile t in unblockedTiles)
            {
                if (!boardCounts.ContainsKey(t.tileId))
                {
                    boardCounts[t.tileId] = 0;
                }
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

        int needed = trayCounts.ContainsKey(targetTileId) ? 3 - trayCounts[targetTileId] : 3;

        foreach (Tile t in unblockedTiles)
        {
            if (t.tileId == targetTileId && highlighted < needed)
            {
                currentlyHintedTiles.Add(t);
                originalSiblingIndices[t] = t.transform.GetSiblingIndex();
                originalScales[t] = t.transform.localScale;
                t.transform.SetAsLastSibling();
                AnimateHint(t.transform);
                highlighted++;
            }
        }
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

        foreach (Tile t in currentlyHintedTiles)
        {
            if (t == null || t.transform == null)
                continue;

            string tweenId =
                t.transform.GetInstanceID() + "_hintScale";

            DOTween.Kill(tweenId);

            if (originalScales.ContainsKey(t))
            {
                t.transform.localScale =
                    originalScales[t];
            }

            if (originalSiblingIndices.ContainsKey(t))
            {
                t.transform.SetSiblingIndex(
                    originalSiblingIndices[t]
                );
            }
        }

        currentlyHintedTiles.Clear();
        originalSiblingIndices.Clear();
        originalScales.Clear();
    }
}