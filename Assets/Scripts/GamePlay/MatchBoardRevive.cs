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

        SoundManager.instance.PlaySound(SoundName.ButtonPop);

        bool adStarted = AdManager.instance.ShowRewardedAd(() =>
                {
                    PerformRevive();
                },
                () =>
                {
                    isReviving = false;

                    Debug.Log("Revive ad was not completed.");
                }
            );

        if (adStarted)
        {
            isReviving = true;
        }
        else
        {
            isReviving = false;
            Debug.Log("Rewarded ad not ready. Revive not started.");
        }
    }

    private void PerformRevive()
    {
        if (BoardSpawner.instance == null ||
            MatchBoard.instance == null ||
            MatchBoardMatch.instance == null)
        {
            isReviving = false;
            return;
        }

        List<GameObject> placedTiles =
            MatchBoard.instance.GetPlacedTiles();

        if (placedTiles.Count == 0)
        {
            isReviving = false;
            return;
        }

        // --------------------------------------------------
        // STEP 1:
        // Take the LAST 3 tiles from the MatchBoard.
        // If fewer than 3 exist, take whatever is available.
        // --------------------------------------------------

        int amountToTake = Mathf.Min(3, placedTiles.Count);

        List<GameObject> matchBoardTilesToRemove =
            new List<GameObject>();

        for (int i = placedTiles.Count - amountToTake;
             i < placedTiles.Count;
             i++)
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

        Dictionary<int, int> removedCounts =
            new Dictionary<int, int>();

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
        // Find matching tiles on the MAIN BOARD so each
        // removed tile type reaches the next multiple of 3.
        //
        // Removed 1 Flower -> remove 2 more Flowers
        // Removed 2 Flowers -> remove 1 more Flower
        // Removed 3 Flowers -> remove 0 more Flowers
        // --------------------------------------------------

        List<GameObject> mainBoardTilesToRemove =
            new List<GameObject>();

        Transform tileParent =
            BoardSpawner.instance.GetTileParent();

        foreach (KeyValuePair<int, int> pair in removedCounts)
        {
            int tileId = pair.Key;
            int removedFromMatchBoard = pair.Value;

            int remainder = removedFromMatchBoard % 3;

            int neededFromMainBoard =
                remainder == 0 ? 0 : 3 - remainder;

            if (neededFromMainBoard <= 0)
                continue;

            foreach (Transform child in tileParent)
            {
                if (neededFromMainBoard <= 0)
                    break;

                if (!child.gameObject.activeSelf)
                    continue;

                Tile tile = child.GetComponent<Tile>();

                if (tile == null)
                    continue;

                if (tile.tileId == tileId &&
                    !tile.IsMoved())
                {
                    mainBoardTilesToRemove.Add(
                        child.gameObject
                    );

                    neededFromMainBoard--;
                }
            }

            if (neededFromMainBoard > 0)
            {
                Debug.LogWarning(
                    $"REVIVE: Could not find enough matching " +
                    $"main-board tiles for Tile ID {tileId}. " +
                    $"Still needed: {neededFromMainBoard}"
                );
            }
        }

        // --------------------------------------------------
        // STEP 4:
        // Remove the 3 selected MatchBoard tiles.
        // --------------------------------------------------

        foreach (GameObject tileObj in matchBoardTilesToRemove)
        {
            if (tileObj == null)
                continue;

            MatchBoard.instance.RemoveTile(tileObj);

            MatchBoardMatch.instance.PlayDestroyEffect(
                tileObj
            );
        }

        // --------------------------------------------------
        // STEP 5:
        // Remove the required matching tiles
        // from the main board.
        // --------------------------------------------------

        foreach (GameObject tileObj in mainBoardTilesToRemove)
        {
            if (tileObj == null)
                continue;

            MatchBoardMatch.instance.PlayDestroyEffect(
                tileObj
            );
        }

        Debug.Log(
            $"REVIVE COMPLETE: " +
            $"Removed {matchBoardTilesToRemove.Count} " +
            $"tiles from MatchBoard and " +
            $"{mainBoardTilesToRemove.Count} " +
            $"matching tiles from main board."
        );

        // Rearrange remaining MatchBoard tiles.
        MatchBoard.instance.RearrangeBoard();

        // Resume gameplay.
        Time.timeScale = 1f;

        GameManager.instance.ResumeGameAfterRevive();

        UIManager.Instance.HidePopup(
            ScreenType.GameOver
        );

        StartCoroutine(
            RefreshTilesAfterRevive()
        );

        isReviving = false;
    }
    private IEnumerator RefreshTilesAfterRevive()
    {
        yield return new WaitForSecondsRealtime(0.25f);

        if (BoardSpawner.instance != null)
        {
            Tile.RefreshAllTileVisuals(BoardSpawner.instance.GetTileParent());
        }
    }
}