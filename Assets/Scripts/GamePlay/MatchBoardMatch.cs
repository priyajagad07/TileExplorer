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
            Tile t = tile.GetComponent<Tile>();
        
            if (t.tileId == tileID && !t.isMatched)
            {
                matched.Add(tile);
            }
        }

        if (matched.Count >= 3)
        {
            SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.MediumImpact);
            removedTiles += 3;

            List<GameObject> tilesToMerge = new List<GameObject>();
            for (int i = 0; i < 3; i++)
            {
                matched[i].GetComponent<Tile>().isMatched = true;
                tilesToMerge.Add(matched[i]);
            }

            activePopAnimation++;
            MergeAndDestroy(tilesToMerge);

            SoundManager.instance.PlaySound(SoundName.ThreeTilesMatch);

            if (ComboManager.instance != null)
            {
                ComboManager.instance.RegisterMatch();
            }
        }
    }

    // ---> NEW HELPER: Forces the tile to the absolute top layer! <---
    void ForceTopLayer(GameObject tileObj, int sortOrder)
    {
        if (tileObj == null) return;
        Canvas canvas = tileObj.GetComponent<Canvas>();
        if (canvas == null) canvas = tileObj.AddComponent<Canvas>();
        
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortOrder;
    }

    void MergeAndDestroy(List<GameObject> matchedTiles)
    {
        if (matchedTiles.Count < 3) return;

        GameObject leftTile = matchedTiles[0];
        GameObject middleTile = matchedTiles[1]; 
        GameObject rightTile = matchedTiles[2];

        // ---> THE LAYER FIX: Bring all 3 to the front, with Middle at the very top! <---
        ForceTopLayer(leftTile, 30000);
        ForceTopLayer(rightTile, 30000);
        ForceTopLayer(middleTile, 30005); 

        RectTransform rectLeft = leftTile.GetComponent<RectTransform>();
        RectTransform rectMid = middleTile.GetComponent<RectTransform>();
        RectTransform rectRight = rightTile.GetComponent<RectTransform>();

        rectLeft.DOKill(); rectMid.DOKill(); rectRight.DOKill();

        Sequence seq = DOTween.Sequence();

        // Phase 1: The Middle Tile rises up and gets a little bigger
        seq.Append(rectMid.DOAnchorPosY(rectMid.anchoredPosition.y + 35f, 0.2f).SetEase(Ease.OutQuad));
        seq.Join(rectMid.DOScale(Vector3.one * 1.25f, 0.2f));

        // Phase 2: The Left and Right tiles get violently sucked into the Middle Tile
        seq.Append(rectLeft.DOMove(rectMid.position, 0.2f).SetEase(Ease.InBack));
        seq.Join(rectRight.DOMove(rectMid.position, 0.2f).SetEase(Ease.InBack));
        
        // (They shrink while being sucked in)
        seq.Join(rectLeft.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
        seq.Join(rectRight.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));

        // Phase 3: The Final Pop! Middle tile swells and explodes!
        seq.Append(rectMid.DOScale(Vector3.one * 1.6f, 0.1f).SetEase(Ease.OutBack));
        seq.Append(rectMid.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack));
        seq.Join(rectMid.DORotate(new Vector3(0, 0, 180f), 0.15f, RotateMode.FastBeyond360).SetEase(Ease.Linear));

        seq.OnComplete(() =>
        {
            SpawnDestroyParticle(middleTile);

            foreach (GameObject tile in matchedTiles)
            {
                MatchBoard.instance.RemoveTile(tile);
                Destroy(tile);
            }

            MatchBoard.instance.RearrangeBoard();
            activePopAnimation--;
        });
    }

    void Rearrange()
    {
        MatchBoard.instance.RearrangeBoard();
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

        if (BoardSpawner.instance == null) return;

        Transform tileParent = BoardSpawner.instance.GetTileParent();
        int boardTiles = 0;

        foreach (Transform child in tileParent)
        {
            if (child == null) continue;
            Tile tile = child.GetComponent<Tile>();
            if (tile == null) continue;
            if (!tile.IsMoved()) boardTiles++;
        }

        int matchTiles = MatchBoard.instance.GetTileCount();

        if (boardTiles <= 0 && matchTiles <= 0)
        {
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
        if (tile == null) return;
        SpawnDestroyParticle(tile);
        Destroy(tile);
    }

    void SpawnDestroyParticle(GameObject tileObj)
    {
        if (tileObj == null) return;

        Vector3 position = tileObj.GetComponent<RectTransform>().position;
        GameObject particle = Instantiate(destroyParticle, position, Quaternion.identity, particleParent);
        activeDestroyParticles++;

        ParticleSystem ps = particle.GetComponent<ParticleSystem>();
        var main = ps.main;
        Tile tileScript = tileObj.GetComponent<Tile>();

        if (tileScript != null && tileScript.particleColors != null && tileScript.particleColors.Length > 0)
        {
            if (tileScript.particleColors.Length == 1)
            {
                main.startColor = tileScript.particleColors[0];
            }
            else if (tileScript.particleColors.Length == 2)
            {
                main.startColor = GetMultiColor(tileScript.particleColors[0], tileScript.particleColors[1]);
            }
            else 
            {
                main.startColor = GetMultiColor(tileScript.particleColors[0], tileScript.particleColors[1], tileScript.particleColors[2]);
            }
        }
        else
        {
            main.startColor = Color.white; 
        }

        float particleDuration = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(particle, particleDuration);
        StartCoroutine(WaitForParticleFinish(particleDuration));
    }

    ParticleSystem.MinMaxGradient GetMultiColor(Color c1, Color c2)
    {
        Gradient gradient = new Gradient();
        gradient.mode = GradientMode.Fixed; 
        gradient.colorKeys = new GradientColorKey[] {
            new GradientColorKey(c1, 0.0f),
            new GradientColorKey(c2, 0.5f)
        };
        
        ParticleSystem.MinMaxGradient minMax = new ParticleSystem.MinMaxGradient(gradient);
        minMax.mode = ParticleSystemGradientMode.RandomColor; 
        return minMax;
    }

    ParticleSystem.MinMaxGradient GetMultiColor(Color c1, Color c2, Color c3)
    {
        Gradient gradient = new Gradient();
        gradient.mode = GradientMode.Fixed;
        gradient.colorKeys = new GradientColorKey[] {
            new GradientColorKey(c1, 0.0f),
            new GradientColorKey(c2, 0.33f),
            new GradientColorKey(c3, 0.66f)
        };
        
        ParticleSystem.MinMaxGradient minMax = new ParticleSystem.MinMaxGradient(gradient);
        minMax.mode = ParticleSystemGradientMode.RandomColor;
        return minMax;
    }
}