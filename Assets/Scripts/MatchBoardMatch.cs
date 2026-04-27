using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchBoardMatch : MonoBehaviour
{
    private MatchBoard board;
    private int removedTiles = 0;
    private int activePopAnimation = 0;

    void Awake()
    {
        board = GetComponent<MatchBoard>();
    }

    public void CheckMatch(List<GameObject> placedTiles, int tileID)
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
                board.RemoveTile(matchtile);
                activePopAnimation++;
                StartCoroutine(PopAndDestroy(matchtile, isFinalMatch));
            }

            Invoke(nameof(Rearrange), 0.6f);
        }
    }

    void Rearrange()
    {
        board.RearrangeBoard();
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
            if (tile == null)
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

        if (checkForWin && activePopAnimation == 0)
        {
            Debug.Log("Level Complete");
            GameManager.instance.LevelComplete();
        }
    }
}