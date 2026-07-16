using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchBoardRevive : MonoBehaviour
{
    private bool isReviving = false;

    public void ReviveBoard()
    {
        if (isReviving)
            return;

        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlaySound(SoundName.ButtonPop);
        }

        if (AdManager.instance == null)
        {
            Debug.LogWarning("REVIVE: AdManager is not available."
            );

            FinishReviveState();
            return;
        }

        bool adStarted =
            AdManager.instance.ShowRewardedAd(() => { PerformRevive(); },
                () =>
                {
                    FinishReviveState();
                    Debug.Log("Revive ad was not completed."
                    );
                }
            );

        if (adStarted)
        {
            isReviving = true;

            if (MatchBoard.instance != null)
            {
                MatchBoard.instance.isInputLocked = true;
            }
        }

        else
        {
            FinishReviveState();
            Debug.Log("Rewarded ad not ready. Revive not started.");
        }
    }

    private void PerformRevive()
    {
        if (BoardSpawner.instance == null ||
            MatchBoard.instance == null ||
            MatchBoardMatch.instance == null)
        {
            FinishReviveState();
            return;
        }

        MatchBoard.instance.CancelPendingBoardChecks();
        List<GameObject> placedTiles = new List<GameObject>(MatchBoard.instance.GetPlacedTiles());

        placedTiles.RemoveAll(tile => tile == null);

        if (placedTiles.Count == 0)
        {
            FinishReviveState();
            return;
        }

        // --------------------------------------------------
        // STEP 1:
        // Take the LAST 3 tiles from the MatchBoard.
        // If fewer than 3 exist, take whatever is available.
        // --------------------------------------------------

        int amountToTake = Mathf.Min(3, placedTiles.Count);

        List<GameObject> matchBoardTilesToRemove = new List<GameObject>();

        for (int i = placedTiles.Count - amountToTake; i < placedTiles.Count; i++)
        {
            GameObject tileObj = placedTiles[i];
            if (tileObj != null)
            {
                matchBoardTilesToRemove.Add(tileObj);
            }
        }

        // --------------------------------------------------
        // STEP 2:
        // Count how many of each tile ID we're removing
        // from the MatchBoard.
        //
        // Example:
        // Last 3 = Flower, Flower, Mango
        //
        // removedCounts:
        // Flower = 2
        // Mango = 1
        // --------------------------------------------------

        Dictionary<int, int> removedCounts = new Dictionary<int, int>();

        foreach (GameObject tileObj in matchBoardTilesToRemove)
        {
            Tile tile = tileObj.GetComponent<Tile>();

            if (tile == null)
                continue;

            if (!removedCounts.ContainsKey(tile.tileId))
            {
                removedCounts[tile.tileId] = 0;
            }

            removedCounts[tile.tileId]++;
        }

        // --------------------------------------------------
        // STEP 3:
        // Build COMPLETE removal groups.
        //
        // A tile type is removed only if enough matching
        // tiles exist to complete its group of 3.
        // --------------------------------------------------

        List<GameObject> validMatchBoardTilesToRemove = new List<GameObject>();
        List<GameObject> mainBoardTilesToRemove = new List<GameObject>();

        Transform tileParent = BoardSpawner.instance.GetTileParent();

        if (tileParent == null)
        {
            Debug.LogWarning("REVIVE: Main board tile parent is null.");
            FinishReviveState();
            return;
        }

        foreach (KeyValuePair<int, int> pair in removedCounts)
        {
            int tileId = pair.Key;
            int removedFromMatchBoard = pair.Value;

            int remainder = removedFromMatchBoard % 3;

            int neededFromMainBoard = remainder == 0 ? 0 : 3 - remainder;
            List<GameObject> foundMainBoardMatches = new List<GameObject>();

            // Find the required matching tiles
            // from the main board.
            if (neededFromMainBoard > 0)
            {
                foreach (Transform child in tileParent)
                {
                    if (foundMainBoardMatches.Count >= neededFromMainBoard)
                    {
                        break;
                    }

                    if (child == null || !child.gameObject.activeSelf)
                    {
                        continue;
                    }

                    Tile tile = child.GetComponent<Tile>();

                    if (tile == null)
                        continue;

                    if (tile.tileId == tileId && !tile.IsMoved())
                    {
                        foundMainBoardMatches.Add(child.gameObject);
                    }
                }
            }

            // Only remove this tile type if its
            // complete group can be created.
            if (foundMainBoardMatches.Count == neededFromMainBoard)
            {
                foreach (GameObject trayTile in matchBoardTilesToRemove)
                {
                    if (trayTile == null)
                        continue;

                    Tile tile = trayTile.GetComponent<Tile>();

                    if (tile != null && tile.tileId == tileId)
                    {
                        validMatchBoardTilesToRemove.Add(trayTile);
                    }
                }

                mainBoardTilesToRemove.AddRange(foundMainBoardMatches);
            }
            else
            {
                Debug.LogWarning($"REVIVE: Skipping Tile ID {tileId}. " + $"Needed {neededFromMainBoard} matching " + $"main-board tiles but found only " + $"{foundMainBoardMatches.Count}.");
            }
        }
        // --------------------------------------------------
        // SAFETY CHECK:
        // Do not change the board if no valid group exists.
        // --------------------------------------------------

        if (validMatchBoardTilesToRemove.Count == 0)
        {
            Debug.LogWarning("REVIVE: No valid complete tile group could be removed.");
            FinishReviveState();
            return;
        }

        // --------------------------------------------------
        // STEP 4:
        // Remove only VALID MatchBoard tiles.
        // --------------------------------------------------

        foreach (GameObject tileObj
                 in validMatchBoardTilesToRemove)
        {
            if (tileObj == null)
                continue;

            MatchBoard.instance.RemoveTile(tileObj);
            MatchBoardMatch.instance.PlayDestroyEffect(tileObj);
        }

        // --------------------------------------------------
        // STEP 5:
        // Remove matching main-board tiles.
        // --------------------------------------------------

        foreach (GameObject tileObj in mainBoardTilesToRemove)
        {
            if (tileObj == null)
                continue;

            MatchBoardMatch.instance.PlayDestroyEffect(tileObj);
        }

        Debug.Log(
            $"REVIVE COMPLETE: " +
            $"Removed {validMatchBoardTilesToRemove.Count} " +
            $"tiles from MatchBoard and " +
            $"{mainBoardTilesToRemove.Count} " +
            $"matching tiles from main board."
        );

        // Rearrange remaining MatchBoard tiles.
        MatchBoard.instance.RearrangeBoard();

        // Resume gameplay.
        Time.timeScale = 1f;

        if (GameManager.instance != null)
        {
            GameManager.instance.ResumeGameAfterRevive();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HidePopup(ScreenType.GameOver);
        }

        StartCoroutine(
            RefreshTilesAfterRevive()
        );

        FinishReviveState();
    }
    private IEnumerator RefreshTilesAfterRevive()
    {
        yield return new WaitForSecondsRealtime(0.25f);

        if (BoardSpawner.instance != null)
        {
            Tile.RefreshAllTileVisuals(BoardSpawner.instance.GetTileParent());
        }

        if (AutoShuffleManager.instance != null)
        {
            AutoShuffleManager.instance.CheckForDeadlock();
        }
    }

    private void FinishReviveState()
    {
        isReviving = false;

        if (MatchBoard.instance != null)
        {
            MatchBoard.instance.isInputLocked = false;
        }
    }
}