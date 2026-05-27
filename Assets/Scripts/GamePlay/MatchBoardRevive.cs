using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchBoardRevive : MonoBehaviour
{
    public void ReviveBoard()
    {
        if (BoardSpawner.instance == null)
            return;

        List<GameObject> placedTiles = MatchBoard.instance.GetPlacedTiles();

        Dictionary<int, int> tileCounts = new Dictionary<int, int>();

        foreach (GameObject tile in placedTiles)
        {
            if (tile == null)
                continue;

            int id = tile.GetComponent<Tile>().tileId;
            if (!tileCounts.ContainsKey(id))
            {
                tileCounts[id] = 0;
            }

            tileCounts[id]++;
        }

        Transform tileParent = BoardSpawner.instance.GetTileParent();
        List<GameObject> tilesToRemove = new List<GameObject>();

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
            MatchBoardMatch.instance.AddRemovedTile();
            MatchBoard.instance.RemoveTile(tile);

            MatchBoardMatch.instance.PlayDestroyEffect(tile);
        }

        placedTiles.Clear();
        MatchBoard.instance.ResetBoard();
        MatchBoard.instance.RearrangeBoard();

        Time.timeScale = 1f;
        UIManager.Instance.HidePopup(ScreenType.GameOver);
    }
}
