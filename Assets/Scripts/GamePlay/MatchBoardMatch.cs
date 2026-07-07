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

    [Header("Effects")]
    [SerializeField] private GameObject destroyParticle;
    [SerializeField] private Transform particleParent;
    [SerializeField] private GameObject slotGlowPrefab;

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
            if (SoundManager.instance != null) SoundManager.instance.ResetPitchTracker();

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

        Transform explosionSlot = middleTile.transform.parent;

        ForceTopLayer(leftTile, 30000);
        ForceTopLayer(rightTile, 30000);
        ForceTopLayer(middleTile, 30005);

        RectTransform rectLeft = leftTile.GetComponent<RectTransform>();
        RectTransform rectMid = middleTile.GetComponent<RectTransform>();
        RectTransform rectRight = rightTile.GetComponent<RectTransform>();

        Vector3 glowWorldPos = rectMid.position;

        rectLeft.DOKill(); rectMid.DOKill(); rectRight.DOKill();

        Sequence seq = DOTween.Sequence();

        // Phase 1: Left and Right tiles get sucked into the Middle Tile FIRST
        seq.Append(rectLeft.DOMove(rectMid.position, 0.15f).SetEase(Ease.InBack));
        seq.Join(rectRight.DOMove(rectMid.position, 0.15f).SetEase(Ease.InBack));
        seq.Join(rectLeft.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack));
        seq.Join(rectRight.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack));

        // Phase 2: Middle tile swells up and rises slightly after absorbing them
        seq.Append(rectMid.DOAnchorPosY(rectMid.anchoredPosition.y + 25f, 0.12f).SetEase(Ease.OutQuad));
        seq.Join(rectMid.DOScale(Vector3.one * 1.4f, 0.12f).SetEase(Ease.OutBack));

        // Phase 3: The Pop!
        seq.Append(rectMid.DOScale(Vector3.zero, 0.12f).SetEase(Ease.InBack));
        seq.Join(rectMid.DORotate(new Vector3(0, 0, 180f), 0.12f, RotateMode.FastBeyond360).SetEase(Ease.Linear));

        seq.OnComplete(() =>
        {
            SpawnDestroyParticle(middleTile);
            // SpawnSlotGlow(explosionSlot);
            SpawnSlotGlow(glowWorldPos);

            Debug.Log(explosionSlot.name);
            Debug.Log(explosionSlot.position);

            foreach (GameObject tile in matchedTiles)
            {
                MatchBoard.instance.RemoveTile(tile);
                Destroy(tile);
            }

            DOVirtual.DelayedCall(0.2f, () =>
            {
                if (MatchBoard.instance != null)
                {
                    MatchBoard.instance.RearrangeBoard();
                }

                activePopAnimation--;
            });
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
            //MatchBoard.instance.isInputLocked = false;
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

    void SpawnSlotGlow(Vector3 worldPos)
    {
        if (slotGlowPrefab == null) return;

        GameObject glow = Instantiate(slotGlowPrefab, particleParent);

        RectTransform rect = glow.GetComponent<RectTransform>();

        rect.position = worldPos;
        rect.localScale = Vector3.one * 0.5f;

        ForceTopLayer(glow, 30001);

        CanvasGroup cg = glow.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = glow.AddComponent<CanvasGroup>();

        cg.alpha = 1f;

        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOScale(Vector3.one * 1.6f, 0.2f).SetEase(Ease.OutCubic));
        seq.Join(cg.DOFade(0f, 0.2f));

        seq.OnComplete(() => Destroy(glow));
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