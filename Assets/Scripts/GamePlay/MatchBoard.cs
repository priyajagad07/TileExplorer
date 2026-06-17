using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MatchBoard : MonoBehaviour
{
    public static MatchBoard instance;
    public List<Transform> slots = new List<Transform>();
    private List<GameObject> placedTiles = new List<GameObject>();
    private MatchBoardMovement movement;
    private MatchBoardMatch matchSystem;
    public bool isInputLocked = false;
    private Tween matchTimer; 

    void Awake()
    {
        instance = this;
        movement = GetComponent<MatchBoardMovement>();
        matchSystem = GetComponent<MatchBoardMatch>();
    }

    public bool AddTile(GameObject tile)
    {
        if (TutorialManager.instance != null)
        {
            if (!TutorialManager.instance.IsTileClickAllowed(tile)) return false;
            TutorialManager.instance.CloseSoftTutorial(); 
        }

        if (placedTiles.Count >= slots.Count)
        {
            return false;
        }

        BoosterSystem.instance.RecordMove(tile);

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

        if (matchTimer != null) matchTimer.Kill();

        matchTimer = DOVirtual.DelayedCall(0.18f, () =>
        {
            ProcessBoard();
            if (BoosterManager.instance != null) BoosterManager.instance.CheckAndUnlockUndoAfterFirstTile();
        });

        return true;
    }

    void ProcessBoard()
    {
        bool matchFound = false;

        for (int i = 0; i < placedTiles.Count; i++)
        {
            if (placedTiles[i] == null) continue;

            int id = placedTiles[i].GetComponent<Tile>().tileId;
            int count = 0;

            foreach (GameObject t in placedTiles)
            {
                if (t != null && t.GetComponent<Tile>().tileId == id) count++;
            }

            if (count >= 3)
            {
                matchSystem.CheckMatch(placedTiles, id);
                matchFound = true;
                break; // Only process one match at a time
            }
        }

        if (matchFound)
        {
            matchTimer = DOVirtual.DelayedCall(0.65f, () => ProcessBoard());
        }
        else
        {
            CheckGameOver();
        }
    }

    void CheckGameOver()
    {
        if (placedTiles.Count >= slots.Count)
        {
            Debug.Log("Game Over");
            GameManager.instance.GameOver();
        }
    }

    public void RearrangeBoard()
    {
        for (int i = 0; i < placedTiles.Count; i++)
        {
            if (placedTiles[i] != null)
            {
                movement.MoveTile(placedTiles[i], slots[i]);
            }
        }
    }

    public void RemoveTile(GameObject tile)
    {
        placedTiles.Remove(tile);
    }

    public void ResetBoard()
    {
        if (matchTimer != null) matchTimer.Kill();

        foreach (GameObject tile in placedTiles)
        {
            if (tile != null)
            {
                // --> FROM OUR RESTART FIX
                tile.transform.DOKill();
                Destroy(tile);
            }
        }

        placedTiles.Clear();

        foreach (Transform slot in slots)
        {
            foreach (Transform child in slot)
            {
                if (child != null)
                {
                    // --> FROM OUR RESTART FIX
                    child.DOKill();
                    Destroy(child.gameObject);
                }
            }
        }
    }

    public List<GameObject> GetPlacedTiles()
    {
        return placedTiles;
    }

    public int GetTileCount()
    {
        return placedTiles.Count;
    }

    public void CleanBoard()
    {
        placedTiles.RemoveAll(tile => tile == null);
    }
}