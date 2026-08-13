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
    private Tween gameOverTimer;

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

        if (tile == null)
        {
            Debug.LogWarning("MatchBoard: Tried to add a null tile.");
            return false;
        }

        Tile tileComponent = tile.GetComponent<Tile>();

        if (tileComponent == null)
        {
            Debug.LogWarning($"MatchBoard: {tile.name} has no Tile component.");
            return false;
        }

        if (movement == null)
        {
            Debug.LogError("MatchBoard: MatchBoardMovement component is missing.");
            return false;
        }

        CleanBoard();
        if (placedTiles.Count >= slots.Count)
        {
            return false;
        }

        // Prevent the same tile from being added twice.
        if (placedTiles.Contains(tile))
        {
            Debug.LogWarning($"MatchBoard: {tile.name} is already in the tray.");
            return false;
        }

        if (IdleHintManager.instance != null)
        {
            IdleHintManager.instance.StopHints();
        }

        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.CloseSoftTutorial();
        }

        int tileID = tileComponent.tileId;
        int insertIndex = -1;

        for (int i = 0; i < placedTiles.Count; i++)
        {
            GameObject existingTileObj = placedTiles[i];

            if (existingTileObj == null)
                continue;

            Tile existingTile = existingTileObj.GetComponent<Tile>();

            if (existingTile == null)
                continue;

            if (existingTile.tileId == tileID && !existingTile.isMatched)
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
        matchTimer?.Kill();
        matchTimer = DOVirtual.DelayedCall(0.18f, ProcessBoard);

        return true;
    }

    void ProcessBoard()
    {
        CleanBoard();

        if (matchSystem == null)
        {
            Debug.LogWarning("MatchBoard: MatchBoardMatch component is missing.");
            return;
        }

        bool matchFound = false;

        for (int i = 0; i < placedTiles.Count; i++)
        {
            GameObject tileObj = placedTiles[i];

            if (tileObj == null)
                continue;

            Tile currentTile = tileObj.GetComponent<Tile>();

            if (currentTile == null)
                continue;

            if (currentTile.isMatched)
                continue;

            int id = currentTile.tileId;
            int count = 0;

            for (int j = 0; j < placedTiles.Count; j++)
            {
                GameObject otherTileObj = placedTiles[j];

                if (otherTileObj == null)
                    continue;

                Tile checkTile = otherTileObj.GetComponent<Tile>();

                if (checkTile == null)
                    continue;

                if (checkTile.tileId == id && !checkTile.isMatched)
                {
                    count++;
                }
            }

            if (count >= 3)
            {
                bool matchStarted = matchSystem.CheckMatch(placedTiles, id);

                if (matchStarted)
                {
                    matchFound = true;
                    break;
                }
            }
        }

        if (matchFound)
        {
            matchTimer?.Kill();
            matchTimer = DOVirtual.DelayedCall(0.65f, ProcessBoard);
        }
        else
        {
            matchTimer = null;
            if (AutoShuffleManager.instance != null)
            {
                AutoShuffleManager.instance.CheckForDeadlock();
            }

            CheckGameOver();
        }
    }

    void CheckGameOver()
    {
        CleanBoard();

        if (placedTiles.Count < slots.Count ||
            HasAnimatingMatchedTile())
        {
            return;
        }

        Debug.Log("Game Over condition met. Waiting for final check...");

        gameOverTimer?.Kill();

        gameOverTimer = DOVirtual.DelayedCall(0.20f, () =>
        {
            gameOverTimer = null;
            CleanBoard();

            if (placedTiles.Count >= slots.Count &&
                !HasAnimatingMatchedTile() &&
                GameManager.instance != null &&
                GameManager.instance.isGameInProgress)
            {
                GameManager.instance.GameOver();
            }
        });
    }

    /// <summary>
    /// True if any tray tile is currently mid-match animation.
    /// </summary>
    private bool HasAnimatingMatchedTile()
    {
        for (int i = 0; i < placedTiles.Count; i++)
        {
            GameObject tileObj = placedTiles[i];

            if (tileObj == null)
                continue;

            Tile tile = tileObj.GetComponent<Tile>();

            if (tile != null && tile.isMatched)
            {
                return true;
            }
        }

        return false;
    }

    public void RearrangeBoard()
    {
        if (movement == null)
        {
            Debug.LogWarning("MatchBoard: MatchBoardMovement component is missing.");
            return;
        }

        CleanBoard();

        int moveCount = Mathf.Min(placedTiles.Count, slots.Count);

        for (int i = 0; i < moveCount; i++)
        {
            GameObject tile = placedTiles[i];
            Transform slot = slots[i];

            if (tile == null || slot == null)
                continue;

            movement.MoveTile(tile, slot);
        }
    }

    public bool RemoveTile(GameObject tile)
    {
        if (tile == null)
            return false;

        return placedTiles.Remove(tile);
    }

    public void ForgetTrayTile(GameObject tile)
    {
        if (tile == null)
            return;

        if (movement != null)
        {
            movement.ForgetTrayTile(tile);
        }
    }

    public void ResetBoard()
    {
        matchTimer?.Kill();
        matchTimer = null;

        gameOverTimer?.Kill();
        gameOverTimer = null;

        // Copy then clear — same observable order as before (clear tray, then despawn).
        List<GameObject> tilesToRemove = new List<GameObject>(placedTiles);

        placedTiles.Clear();

        if (movement != null)
        {
            movement.ResetMovementState();
        }

        for (int i = 0; i < tilesToRemove.Count; i++)
        {
            GameObject tile = tilesToRemove[i];
            if (tile == null)
                continue;

            tile.transform.DOKill();
            RectTransform rect = tile.transform as RectTransform;

            if (rect != null)
            {
                rect.DOKill();
            }

            Tile tileComponent = tile.GetComponent<Tile>();

            if (tileComponent != null)
            {
                tileComponent.isMatched = false;
            }

            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.Despawn(tile);
            }
            else
            {
                Debug.LogWarning("MatchBoard: ObjectPoolManager is missing.");
            }
        }

        isInputLocked = false;
        Debug.Log($"MATCH BOARD RESET: Removed {tilesToRemove.Count} tiles.");
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
        // Reverse scan avoids RemoveAll lambda / delegate allocation.
        for (int i = placedTiles.Count - 1; i >= 0; i--)
        {
            if (placedTiles[i] == null)
            {
                placedTiles.RemoveAt(i);
            }
        }
    }

    public void CancelPendingBoardChecks()
    {
        matchTimer?.Kill();
        matchTimer = null;

        gameOverTimer?.Kill();
        gameOverTimer = null;
    }
}
