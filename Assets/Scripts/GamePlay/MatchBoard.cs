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
        DOVirtual.DelayedCall(0.16f, () =>
            {
                matchSystem.CheckMatch(
                placedTiles,
                tileID
            );

                CheckGameOver();
            }
        );
        return true;
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
            movement.MoveTile(placedTiles[i], slots[i]);
        }
    }

    public void RemoveTile(GameObject tile)
    {
        placedTiles.Remove(tile);
    }

    public void ResetBoard()
    {
        foreach (GameObject tile in placedTiles)
        {
            if (tile != null)
            {
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