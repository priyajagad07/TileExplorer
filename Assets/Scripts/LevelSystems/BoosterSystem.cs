using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void UndoMove()
    {
        if (undoStack.Count == 0)
            return;

        UndoData data = undoStack.Pop();

        if (data.tile == null)
            return;

        MatchBoard.instance.RemoveTile(data.tile);

        data.tile.transform.SetParent(data.originalParent);
        data.tile.transform.SetSiblingIndex(data.siblingIndex);

        RectTransform rect =
            data.tile.GetComponent<RectTransform>();

        rect.anchoredPosition = data.originalPosition;

        StartCoroutine(
            UndoBounce(data.tile.transform)
        );

        Tile tileScript = data.tile.GetComponent<Tile>();
        tileScript.SetMoved(false);

        MatchBoard.instance.RearrangeBoard();

        SoundManager.instance.PlaySound(SoundName.TileMoveToBoard);
    }

    IEnumerator UndoBounce(Transform tile)
    {
        Vector3 originalScale = tile.localScale;

        float time = 0;
        float duration = 0.25f;

        while (time < duration)
        {
            float t = time / duration;

            tile.localScale =
                Vector3.Lerp(
                    originalScale * 1.25f,
                    originalScale,
                    t
                );

            time += Time.deltaTime;
            yield return null;
        }

        tile.localScale = originalScale;
    }

    public void ClearUndoStack()
    {
        undoStack.Clear();
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

        List<Tile> tiles = new List<Tile>();

        foreach (Transform child in tileParent)
        {
            if (child == null)
                continue;

            Tile tile = child.GetComponent<Tile>();

            if (tile == null)
                continue;

            if (tile.IsMoved())
                continue;

            tiles.Add(tile);
        }

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

        for (int i = 0; i < tiles.Count; i++)
        {
            RectTransform rect = tiles[i].GetComponent<RectTransform>();
            StartCoroutine(AnimateShuffle(rect, positions[i]));
        }

        SoundManager.instance.PlaySound(SoundName.TileMoveToBoard);
    }

    IEnumerator AnimateShuffle(RectTransform rect, Vector2 targetPos)
    {
        Vector2 startPos = rect.anchoredPosition;

        float time = 0;
        float duration = 0.35f;

        float targetRotation = Random.Range(-30f, 30f);

        while (time < duration)
        {
            float t = time / duration;

            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

            rect.localRotation = Quaternion.Euler(0, 0,
                Mathf.Lerp(0, targetRotation, t));

            time += Time.deltaTime;
            yield return null;
        }

        rect.anchoredPosition = targetPos;
        rect.rotation = Quaternion.identity;
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

                    tile.MoveToBoard();
                    StartCoroutine(MagicMove(tile));
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
                    group.Value[i].MoveToBoard();
                }
                SoundManager.instance.PlaySound(SoundName.ThreeTilesMatch);
                return;
            }
        }

        Debug.Log("No valid magic match possible on the board");
    }

    IEnumerator MagicMove(Tile tile)
    {
        Transform t = tile.transform;

        Vector3 originalScale = t.localScale;
        Vector3 targetScale = originalScale * 1.2f;

        float time = 0;
        float duration = 0.18f;

        while (time < duration)
        {
            t.localScale = Vector3.Lerp(originalScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        t.localScale = originalScale;
        tile.MoveToBoard();
    }
}