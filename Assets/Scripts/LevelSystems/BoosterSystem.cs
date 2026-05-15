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

        RectTransform rect = data.tile.GetComponent<RectTransform>();
        rect.anchoredPosition = data.originalPosition;

        Tile tileScript = data.tile.GetComponent<Tile>();
        tileScript.SetMoved(false);

        MatchBoard.instance.RearrangeBoard();

        SoundManager.instance.PlaySound(SoundName.TileMoveToBoard);
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
        List<int> siblingIndices = new List<int>();

        foreach (Tile tile in tiles)
        {
            positions.Add(tile.GetComponent<RectTransform>().anchoredPosition);
            siblingIndices.Add(tile.transform.GetSiblingIndex());
        }

        for (int i = 0; i < positions.Count; i++)
        {
            int randomIndex = Random.Range(i, positions.Count);

            Vector2 tempPos = positions[i];
            positions[i] = positions[randomIndex];
            positions[randomIndex] = tempPos;

            int tempIndex = siblingIndices[i];
            siblingIndices[i] = siblingIndices[randomIndex];
            siblingIndices[randomIndex] = tempIndex;
        }

        for (int i = 0; i < tiles.Count; i++)
        {
            RectTransform rect = tiles[i].GetComponent<RectTransform>();
            rect.anchoredPosition = positions[i];

            tiles[i].transform.SetSiblingIndex(siblingIndices[i]);
        }

        SoundManager.instance.PlaySound(SoundName.TileMoveToBoard);
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
}