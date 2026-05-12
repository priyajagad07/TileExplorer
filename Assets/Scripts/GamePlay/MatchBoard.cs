using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchBoard : MonoBehaviour
{
    public static MatchBoard instance;
    public List<Transform> slots = new List<Transform>();
    private List<GameObject> placedTiles = new List<GameObject>();
    private MatchBoardMovement movement;
    private MatchBoardMatch matchSystem;
    private Stack<UndoData> undoStack = new Stack<UndoData>();

    void Awake()
    {
        instance = this;
        movement = GetComponent<MatchBoardMovement>();
        matchSystem = GetComponent<MatchBoardMatch>();
    }

    public bool AddTile(GameObject tile)
    {
        if (placedTiles.Count >= slots.Count)
        {
            return false;
        }

        RectTransform rect = tile.GetComponent<RectTransform>();
        UndoData data = new UndoData(
            tile,
            tile.transform.parent,
            rect.anchoredPosition,
            tile.transform.GetSiblingIndex()
        );

        undoStack.Push(data);

        int tileID = tile.GetComponent<Tile>().tileId;
        int insertIndex = -1;

        for (int i = 0; i < placedTiles.Count; i++)
        {
            if (placedTiles[i].GetComponent<Tile>().tileId == tileID)
            {
                insertIndex = i + 1;
            }
        }

        if (insertIndex == -1)
        {
            placedTiles.Add(tile);
        }
        else
        {
            placedTiles.Insert(insertIndex, tile);
        }

        RearrangeBoard();
        matchSystem.CheckMatch(placedTiles, tileID);

        return true;
    }

    public void RearrangeBoard()
    {
        for (int i = 0; i < placedTiles.Count; i++)
        {
            movement.MoveTile(placedTiles[i], slots[i]);
        }
    }

    public void ResetBoard()
    {
        placedTiles.Clear();

        foreach (Transform slot in slots)
        {
            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public List<GameObject> GetPlacedTiles()
    {
        return placedTiles;
    }

    public void RemoveTile(GameObject tile)
    {
        placedTiles.Remove(tile);
    }

    public int GetTileCount()
    {
        return placedTiles.Count;
    }

    public void UndoMove()
    {
        if (!BoosterManager.instance.UseUndo())
        {
            Debug.Log("No Undo Left");
            UIManager.Instance.ShowPopup(ScreenType.BuyUndoScreen);
            return;
        }

        if (undoStack.Count == 0)
            return;

        UndoData data = undoStack.Pop();

        if (data.tile == null)
            return;

        placedTiles.Remove(data.tile);

        data.tile.transform.SetParent(data.originalParent);
        data.tile.transform.SetSiblingIndex(data.siblingIndex);

        RectTransform rect = data.tile.GetComponent<RectTransform>();
        rect.anchoredPosition = data.originalPosition;

        Tile tileScript = data.tile.GetComponent<Tile>();
        tileScript.SetMoved(false);

        RearrangeBoard();

        SoundManager.instance.PlaySound(SoundName.TileMoveToBoard);
    }

    public void ClearUndoStack()
    {
        undoStack.Clear();
    }

    public void ShuffleTiles()
    {
        if (!BoosterManager.instance.UseShuffle())
        {
            Debug.Log("No Shuffle Left");
            UIManager.Instance.ShowPopup(ScreenType.BuyShuffleScreen);
            return;
        }

        BoosterSystem.instance.ShuffleTiles();
    }

    public void UseMagic()
    {
        if (!BoosterManager.instance.UseMagic())
        {
            Debug.Log("No Magic Left");
            UIManager.Instance.ShowPopup(ScreenType.BuyMagicScreen);
            return;
        }

        BoosterSystem.instance.UseMagicBooster();
    }

    public void ReviveBoard()
    {
        List<int> tileIds = new List<int>();

        foreach (GameObject tile in placedTiles)
        {
            tileIds.Add(tile.GetComponent<Tile>().tileId);
        }

        Transform tileParent = BoardSpawner.instance.GetTileParent();

        List<GameObject> tilesToRemove = new List<GameObject>();

        Dictionary<int, int> tileCounts = new Dictionary<int, int>();

        foreach (GameObject tile in placedTiles)
        {
            int id = tile.GetComponent<Tile>().tileId;

            if (!tileCounts.ContainsKey(id))
            {
                tileCounts[id] = 0;
            }

            tileCounts[id]++;
        }

        foreach (var pair in tileCounts)
        {
            int tileId = pair.Key;
            int currentCount = pair.Value;

            int needed = 3 - currentCount;

            foreach (Transform child in tileParent)
            {
                if (needed <= 0)
                    break;

                Tile tile = child.GetComponent<Tile>();

                if (tile == null)
                    continue;

                if (tile.tileId == tileId)
                {
                    tilesToRemove.Add(child.gameObject);
                    MatchBoardMatch.instance.AddRemovedTile();
                    needed--;
                }
            }
        }

        foreach (GameObject tile in placedTiles)
        {
            tilesToRemove.Add(tile);
        }

        foreach (GameObject tile in tilesToRemove)
        {
            Destroy(tile);
        }

        StartCoroutine(CheckReviveWin());

        placedTiles.Clear();
        RearrangeBoard();

        Time.timeScale = 1f;
        UIManager.Instance.HidePopup(ScreenType.GameOver);
    }

    IEnumerator CheckReviveWin()
    {
        yield return new WaitForSeconds(0.2f);

        Transform tileParent = BoardSpawner.instance.GetTileParent();

        if(tileParent.childCount <= 0)
        {
            GameManager.instance.LevelComplete();
        }
    }
}