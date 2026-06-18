using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchBoardRevive : MonoBehaviour
{
    public void ReviveBoard()
    {
        SoundManager.instance.PlaySound(SoundName.ButtonPop);

        AdManager.instance.ShowRewardedAd(() =>
        {
            if (BoardSpawner.instance == null)
                return;

            List<GameObject> placedTiles = MatchBoard.instance.GetPlacedTiles();
            if (placedTiles.Count == 0) return;

            HashSet<int> targetTileIds = new HashSet<int>();
            int tilesToCheck = Mathf.Min(3, placedTiles.Count);

            for (int i = 0; i < tilesToCheck; i++)
            {
                int index = placedTiles.Count - 1 - i;
                if (placedTiles[index] != null)
                {
                    Tile tile = placedTiles[index].GetComponent<Tile>();
                    if (tile != null)
                    {
                        targetTileIds.Add(tile.tileId);
                    }
                }
            }

            List<GameObject> tilesToDestroy = new List<GameObject>();
            Transform tileParent = BoardSpawner.instance.GetTileParent();

            foreach (int id in targetTileIds)
            {
                List<GameObject> matchedGroup = new List<GameObject>();

                foreach (GameObject t in placedTiles)
                {
                    if (t != null && t.GetComponent<Tile>().tileId == id)
                    {
                        matchedGroup.Add(t);
                    }
                }

                if (matchedGroup.Count < 3)
                {
                    foreach (Transform child in tileParent)
                    {
                        if (matchedGroup.Count >= 3) break;

                        Tile boardTile = child.GetComponent<Tile>();

                        if (boardTile != null && boardTile.tileId == id && !boardTile.IsMoved())
                        {
                            matchedGroup.Add(child.gameObject);
                        }
                    }
                }

                tilesToDestroy.AddRange(matchedGroup);
            }

            foreach (GameObject tileObj in tilesToDestroy)
            {
                if (placedTiles.Contains(tileObj))
                {
                    MatchBoard.instance.RemoveTile(tileObj);
                }

                MatchBoardMatch.instance.AddRemovedTile();
                MatchBoardMatch.instance.PlayDestroyEffect(tileObj);
            }

            MatchBoard.instance.RearrangeBoard();

            Time.timeScale = 1f;
            StartCoroutine(RefreshTilesAfterRevive(tileParent));
            UIManager.Instance.HidePopup(ScreenType.GameOver);
        });
    }

    IEnumerator RefreshTilesAfterRevive(Transform tileParent)
    {
        yield return new WaitForSeconds(0.25f);
        Tile.RefreshAllTileVisuals(tileParent);
    }
}