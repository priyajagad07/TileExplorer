using System.Collections.Generic;
using UnityEngine;

public class BoosterSystem : MonoBehaviour
{
    public static BoosterSystem instance;

    void Awake()
    {
        instance = this;
    }

    public void ShuffleTiles()
    {
        Debug.Log("Shuffle is working");

        List<Tile> remainingTiles = new List<Tile>();

        Transform tileParent = BoardSpawner.instance.GetTileParent();

        foreach (Transform child in tileParent)
        {
            Tile tile = child.GetComponent<Tile>();
            if (tile != null)
            {
                remainingTiles.Add(tile);
            }
        }

        List<Vector2> positions = new List<Vector2>();

        foreach (Tile tile in remainingTiles)
        {
            RectTransform rect = tile.GetComponent<RectTransform>();
            positions.Add(rect.anchoredPosition);
        }

        for (int i = 0; i < remainingTiles.Count; i++)
        {
            Tile temp = remainingTiles[i];

            int randomIndex = Random.Range(i, remainingTiles.Count);
            remainingTiles[i] = remainingTiles[randomIndex];
            remainingTiles[randomIndex] = temp;
        }

        for (int i = 0; i < remainingTiles.Count; i++)
        {
            RectTransform rect = remainingTiles[i].GetComponent<RectTransform>();
            rect.anchoredPosition = positions[i];
        }

        SoundManager.instance.PlaySound(SoundName.TileMoveToBoard);
    }

    public void UseMagicBooster()
    {
        Dictionary<int, List<Tile>> tileGroups = new Dictionary<int, List<Tile>>();

        Transform tileParent = BoardSpawner.instance.GetTileParent();

        foreach (Transform child in tileParent)
        {
            Tile tile = child.GetComponent<Tile>();

            if (tile == null)
                continue;

            if (tile.IsMoved())
                continue;

            if (!tileGroups.ContainsKey(tile.tileId))
            {
                tileGroups.Add(tile.tileId, new List<Tile>());
            }

            tileGroups[tile.tileId].Add(tile);
        }

        foreach (var group in tileGroups)
        {
            if (group.Value.Count >= 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    group.Value[i].MoveToBoard();
                }

                SoundManager.instance.PlaySound(SoundName.ThreeTilesMatch);
                return;
            }
        }

        Debug.Log("No valid match");
    }
}
