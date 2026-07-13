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
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        movement = GetComponent<MatchBoardMovement>();
        matchSystem = GetComponent<MatchBoardMatch>();
    }

    public bool AddTile(GameObject tile)
    {
        if (isInputLocked)
            return false;

        if (IdleHintManager.instance != null)
        {
            IdleHintManager.instance.StopHints();
        }

        if (TutorialManager.instance != null)
        {
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
            Tile t = placedTiles[i].GetComponent<Tile>();
            if (t.tileId == tileID && !t.isMatched)
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
        });

        return true;
    }

    void ProcessBoard()
    {
        bool matchFound = false;

        for (int i = 0; i < placedTiles.Count; i++)
        {
            if (placedTiles[i] == null) continue;
            Tile currentTile = placedTiles[i].GetComponent<Tile>();
            if (currentTile.isMatched) continue;

            int id = currentTile.tileId;
            int count = 0;

            foreach (GameObject t in placedTiles)
            {
                if (t != null)
                {
                    Tile checkTile = t.GetComponent<Tile>();
                    if (checkTile.tileId == id && !checkTile.isMatched) count++;
                }
            }

            if (count >= 3)
            {
                matchSystem.CheckMatch(placedTiles, id);
                matchFound = true;
                break;
            }
        }

        if (matchFound)
        {
            matchTimer = DOVirtual.DelayedCall(0.65f, () => ProcessBoard());
        }
        else
        {
            if (AutoShuffleManager.instance != null)
            {
                AutoShuffleManager.instance.CheckForDeadlock();
            }
            CheckGameOver();
        }
    }

    void CheckGameOver()
    {
        bool isAnimating = false;
        foreach (GameObject t in placedTiles)
        {
            if (t != null && t.GetComponent<Tile>().isMatched) isAnimating = true;
        }

        if (placedTiles.Count >= slots.Count && !isAnimating)
        {
            Debug.Log("Game Over condition met. Waiting for animations to finish...");

            DOVirtual.DelayedCall(0.20f, () =>
            {
                if (placedTiles.Count >= slots.Count)
                {
                    GameManager.instance.GameOver();
                }
            });
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
                tile.transform.DOKill();
                ObjectPoolManager.Instance.Despawn(tile);
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
                    // Make sure this is Despawn, NOT Destroy!
                    ObjectPoolManager.Instance.Despawn(child.gameObject);
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