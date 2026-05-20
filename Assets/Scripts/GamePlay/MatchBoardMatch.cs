using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchBoardMatch : MonoBehaviour
{
    public static MatchBoardMatch instance;
    private int removedTiles = 0;
    private int activePopAnimation = 0;
    [SerializeField] private GameObject destroyParticle;
    [SerializeField] private Transform particleParent;

    void Awake()
    {
        instance = this;
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

            BoosterSystem.instance.ClearUndoStack();
            foreach (GameObject matchtile in matched)
            {
                MatchBoard.instance.RemoveTile(matchtile);
                activePopAnimation++;
                StartCoroutine(PopAndDestroy(matchtile, isFinalMatch));
            }

            Invoke(nameof(Rearrange), 0.6f);

            SoundManager.instance.PlaySound(SoundName.ThreeTilesMatch);
        }
    }

    void Rearrange()
    {
        MatchBoard.instance.RearrangeBoard();
    }

    IEnumerator PopAndDestroy(GameObject tile, bool checkForWin)
    {
        RectTransform rect = tile.GetComponent<RectTransform>();

        float time = 0f;
        float duration = 0.2f;

        //scale up(Pop Effect)
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.one * 1.45f;

        while (time < duration)
        {
            if (tile == null)
                yield break;

            rect.localScale = Vector3.Lerp(startScale, endScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.08f);

        //Scale Dowm to zero
        time = 0f;
        while (time < duration)
        {
            rect.localScale = Vector3.Lerp(endScale, Vector3.zero, time / duration);
            rect.Rotate(0, 0, 12f);
            time += Time.deltaTime;
            yield return null;
        }

        GameObject particle = Instantiate(destroyParticle, rect.position, Quaternion.identity, particleParent);
        Destroy(particle, 2f);

        Destroy(tile);

        yield return new WaitForSeconds(2f);

        activePopAnimation--;


        StartCoroutine(CheckCompletionDelayed());
    }

    IEnumerator CheckCompletionDelayed()
    {
        yield return null;
        yield return null;

        CheckLevelComplete();
    }

    public void ResetBoardState()
    {
        StopAllCoroutines();
        removedTiles = 0;
        activePopAnimation = 0;
    }

    public void AddRemovedTile()
    {
        removedTiles++;
    }

    public void CheckLevelComplete()
    {
        MatchBoard.instance.CleanBoard();

        if (BoardSpawner.instance == null)
        {
            Debug.LogError("Win Check Failed: BoardSpawner is null!");
            return;
        }

        Transform tileParent = BoardSpawner.instance.GetTileParent();
        int boardTiles = 0;

        foreach (Transform child in tileParent)
        {
            if (child == null)
                continue;

            Tile tile = child.GetComponent<Tile>();

            if (tile == null)
                continue;

            if (!tile.IsMoved())
            {
                boardTiles++;
            }
        }

        int matchTiles = MatchBoard.instance.GetTileCount();

        if (boardTiles <= 0 && matchTiles <= 0)
        {
            Debug.Log("level Complete");
            GameManager.instance.LevelComplete();
        }
    }
}