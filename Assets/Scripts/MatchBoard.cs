using System.Collections.Generic;
using UnityEngine;

public class MatchBoard : MonoBehaviour
{
    public List<Transform> slots = new List<Transform>();
    private List<GameObject> placedTiles = new List<GameObject>();

    public bool AddTile(GameObject tile)
    {
        if (placedTiles.Count >= slots.Count)
        {
            Debug.Log("Game Over");
            GameManager.instance.GameOver();
            return false;
        }

        Transform targetSlot = slots[placedTiles.Count];

        tile.transform.SetParent(targetSlot);

        RectTransform rect = tile.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;

        placedTiles.Add(tile);

        CheckMatch(tile.GetComponent<Tile>().tileId);
        return true;
    }

    void CheckMatch(int tileID)
    {
        List<GameObject> matched = new List<GameObject>();

        foreach (GameObject tile in placedTiles)
        {
            if (tile.GetComponent<Tile>().tileId == tileID)
            {
                matched.Add(tile);
            }
        }

        if (matched.Count >= 3)
        {
            foreach (GameObject matchtile in matched)
            {
                placedTiles.Remove(matchtile);
                Destroy(matchtile);
            }

            RearrangeBoard();
        }

        if(placedTiles.Count == 0 && FindObjectsOfType<Tile>().Length == 0)
        {
            GameManager.instance.LevelComplete();
        }
    }
    
    void RearrangeBoard()
    {
        for (int i = 0; i < placedTiles.Count; i++)
        {
            Transform targetSlot = slots[i];
            GameObject tile = placedTiles[i];

            tile.transform.SetParent(targetSlot);

            RectTransform rect = tile.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
        }
    }
}