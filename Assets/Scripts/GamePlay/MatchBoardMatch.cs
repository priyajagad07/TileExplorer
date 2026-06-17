using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Solo.MOST_IN_ONE;

public class MatchBoardMatch : MonoBehaviour
{
    public static MatchBoardMatch instance;
    private int removedTiles = 0;
    private int activePopAnimation = 0;
    [SerializeField] private GameObject destroyParticle;
    [SerializeField] private Transform particleParent;
    private int activeDestroyParticles = 0;

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
            SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.MediumImpact);
            removedTiles += 3;

            for (int i = 0; i < 3; i++)
            {
                GameObject matchtile = matched[i];
                MatchBoard.instance.RemoveTile(matchtile);
                activePopAnimation++;
                PopAndDestroy(matchtile);
            }

            Invoke(nameof(Rearrange), 0.6f);

            SoundManager.instance.PlaySound(SoundName.ThreeTilesMatch);
        }
    }

    void Rearrange()
    {
        MatchBoard.instance.RearrangeBoard();
    }

    void PopAndDestroy(GameObject tile)
    {
        if (tile == null)
            return;

        RectTransform rect = tile.GetComponent<RectTransform>();

        rect.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Append(rect.DOScale(Vector3.one * 1.45f, 0.2f).SetEase(Ease.OutBack));
        seq.AppendInterval(0.08f);
        seq.Append(rect.DOScale(Vector3.zero, 0.22f).SetEase(Ease.InBack));
        seq.Join(rect.DORotate(new Vector3(0, 0, 90f), 0.22f, RotateMode.FastBeyond360).SetEase(Ease.Linear));

        seq.OnComplete(() =>
        {
            SpawnDestroyParticle(
                rect.position
            );

            Destroy(tile);

            activePopAnimation--;
        });
    }
    public void ResetBoardState()
    {
        StopAllCoroutines();
        removedTiles = 0;
        activePopAnimation = 0;
        activeDestroyParticles = 0;
        CancelInvoke(nameof(Rearrange));
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

    IEnumerator WaitForParticleFinish(float duration)
    {
        yield return new WaitForSeconds(duration);

        activeDestroyParticles--;

        if (activePopAnimation <= 0 && activeDestroyParticles <= 0)
        {
            CheckLevelComplete();
        }
    }

    public void PlayDestroyEffect(GameObject tile)
    {
        if (tile == null)
            return;

        RectTransform rect = tile.GetComponent<RectTransform>();
        SpawnDestroyParticle(rect.position);
        Destroy(tile);
    }

    void SpawnDestroyParticle(Vector3 position)
    {
        GameObject particle = Instantiate(destroyParticle, position, Quaternion.identity, particleParent);
        activeDestroyParticles++;

        ParticleSystem ps = particle.GetComponent<ParticleSystem>();
        float particleDuration = ps.main.duration + ps.main.startLifetime.constantMax;

        Destroy(particle, particleDuration);

        StartCoroutine(WaitForParticleFinish(particleDuration));
    }
}