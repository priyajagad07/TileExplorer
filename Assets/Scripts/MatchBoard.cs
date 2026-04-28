using System.Collections.Generic;
using UnityEngine;

public class MatchBoard : MonoBehaviour
{
    public static MatchBoard instance;
    public List<Transform> slots = new List<Transform>();
    private List<GameObject> placedTiles = new List<GameObject>();
    private MatchBoardMovement movement;
    private MatchBoardMatch matchSystem;

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

        foreach(Transform slot in slots)
        {
           foreach(Transform child in slot)
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
}