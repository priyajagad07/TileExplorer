using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchBoard : MonoBehaviour
{
    public List<Transform> slots = new List<Transform>();
    private List<GameObject> placedTiles = new List<GameObject>();
    private int removedTiles = 0;
    private int activePopAnimation = 0;

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
        CheckMatch(tileID);

        return true;
    }

    void RearrangeBoard()
    {
        for (int i = 0; i < placedTiles.Count; i++)
        {
            StartCoroutine(MoveToSlot(placedTiles[i], slots[i]));
        }
    }

    IEnumerator MoveToSlot(GameObject tile, Transform targetSlot)
    {
        RectTransform rect = tile.GetComponent<RectTransform>();

        Vector3 startPos = rect.position;

        float time = 0f;
        float duration = 0.15f;

        while (time < duration)
        {
            if (tile == null)
                yield break;

            rect.position = Vector3.Lerp(startPos, targetSlot.position, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        rect.position = targetSlot.position;
        tile.transform.SetParent(targetSlot);
        rect.anchoredPosition = Vector2.zero;

        if (placedTiles.Count >= slots.Count)
        {
            Debug.Log("Game Over");
            GameManager.instance.GameOver();
        }
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
            removedTiles += matched.Count;

            bool isFinalMatch = removedTiles >= BoardGenerator.totalTilesInLevel;

            foreach (GameObject matchtile in matched)
            {
                placedTiles.Remove(matchtile);
                activePopAnimation++;
                StartCoroutine(PopAndDestroy(matchtile, isFinalMatch));
            }

            Invoke("RearrangeBoard", 0.6f);

            if (isFinalMatch)
            {
                StartCoroutine(LevelComplete());
            }
        }
    }

    IEnumerator LevelComplete()
    {
        yield return new WaitForSeconds(0.9f);

        Debug.Log("Level Completed");
        GameManager.instance.LevelComplete();
    }

    IEnumerator PopAndDestroy(GameObject tile, bool checkForWin)
    {
        RectTransform rect = tile.GetComponent<RectTransform>();

        float time = 0f;
        float duration = 0.2f;

        //scale up(Pop Effect)
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.one * 1.2f;

        while (time < duration)
        {
            if(tile == null)
                yield break;

            rect.localScale = Vector3.Lerp(startScale, endScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        //Scale Dowm to zero
        time = 0f;
        while (time < duration)
        {
            rect.localScale = Vector3.Lerp(endScale, Vector3.zero, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        Destroy(tile);

        activePopAnimation--;

        if(checkForWin && activePopAnimation == 0)
        {
            Debug.Log("Level Complete");
            GameManager.instance.LevelComplete();
        }
    }
}
