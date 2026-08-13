using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Solo.MOST_IN_ONE;

public class MatchBoardMatch : MonoBehaviour
{
    public static MatchBoardMatch instance;
    //private int removedTiles = 0;
    private int activePopAnimation = 0;
    private int boardStateVersion = 0;
    private readonly List<Tween> activeDelayedCalls = new List<Tween>();
    private readonly List<Sequence> activeMatchSequences = new List<Sequence>();
    private readonly List<Sequence> activeGlowSequences = new List<Sequence>();
    private readonly List<GameObject> activeGlowObjects = new List<GameObject>();
    private readonly List<GameObject> activeParticleObjects = new List<GameObject>();

    // Reused per CheckMatch to avoid allocating match candidate lists every call.
    private readonly List<GameObject> matchedBuffer = new List<GameObject>(8);
    private readonly List<GameObject> tilesToMergeBuffer = new List<GameObject>(3);

    [Header("Effects")]
    [SerializeField] private GameObject destroyParticle;
    [SerializeField] private Transform particleParent;
    [SerializeField] private GameObject slotGlowPrefab;

    [Header("Animation Layer")]
    [SerializeField] private RectTransform matchAnimationLayer;

    private int activeDestroyParticles = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool CheckMatch(List<GameObject> placedTiles, int tileID)
    {
        matchedBuffer.Clear();

        for (int i = 0; i < placedTiles.Count; i++)
        {
            GameObject tile = placedTiles[i];
            if (tile == null)
                continue;

            Tile t = tile.GetComponent<Tile>();

            if (t == null)
                continue;

            if (t.tileId == tileID && !t.isMatched)
            {
                matchedBuffer.Add(tile);
            }
        }

        if (matchedBuffer.Count >= 3)
        {
            if (SoundManager.instance != null)
            {
                SoundManager.instance.ResetPitchTracker();
                SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.MediumImpact
                );
            }

            tilesToMergeBuffer.Clear();

            Tile mergeTile0 = null;
            Tile mergeTile1 = null;
            Tile mergeTile2 = null;

            for (int i = 0; i < 3; i++)
            {
                GameObject tileObj = matchedBuffer[i];

                if (tileObj == null)
                    return false;

                Tile tileScript = tileObj.GetComponent<Tile>();
                if (tileScript == null)
                    return false;

                tilesToMergeBuffer.Add(tileObj);

                if (i == 0) mergeTile0 = tileScript;
                else if (i == 1) mergeTile1 = tileScript;
                else mergeTile2 = tileScript;
            }

            mergeTile0.isMatched = true;
            mergeTile1.isMatched = true;
            mergeTile2.isMatched = true;

            activePopAnimation++;

            // Pass a snapshot list into the async merge so reused buffers
            // cannot be mutated while the DOTween sequence is running.
            List<GameObject> tilesToMerge =
                new List<GameObject>(tilesToMergeBuffer);

            bool matchStarted =
                MergeAndDestroy(tilesToMerge);

            if (!matchStarted)
            {
                return false;
            }

            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlaySound(
                    SoundName.ThreeTilesMatch
                );
            }

            if (ComboManager.instance != null)
            {
                ComboManager.instance.RegisterMatch();
            }
            return true;
        }
        return false;
    }

    private void MoveToAnimationLayer(GameObject tileObj)
    {
        if (tileObj == null || matchAnimationLayer == null)
            return;

        RectTransform rect =
            tileObj.transform as RectTransform;

        if (rect == null)
            return;

        // true preserves the tile's current world position.
        rect.SetParent(matchAnimationLayer, true);
        rect.SetAsLastSibling();
    }

    bool MergeAndDestroy(List<GameObject> matchedTiles)
    {
        if (matchedTiles == null ||
            matchedTiles.Count < 3)
        {
            return false;
        }

        int capturedVersion = boardStateVersion;

        GameObject leftTile = matchedTiles[0];
        GameObject middleTile = matchedTiles[1];
        GameObject rightTile = matchedTiles[2];

        if (leftTile == null || middleTile == null || rightTile == null)
        {
            CancelMatchedTiles(matchedTiles);
            return false;
        }

        RectTransform rectLeft = leftTile.transform as RectTransform;
        RectTransform rectMid = middleTile.transform as RectTransform;
        RectTransform rectRight = rightTile.transform as RectTransform;

        if (rectLeft == null || rectMid == null || rectRight == null)
        {
            CancelMatchedTiles(matchedTiles);
            return false;
        }

        // Capture before changing parent.
        Vector3 glowWorldPos = rectMid.position;

        MoveToAnimationLayer(leftTile);
        MoveToAnimationLayer(rightTile);
        MoveToAnimationLayer(middleTile);

        // Keep the middle tile above the other two.
        rectLeft.SetAsLastSibling();
        rectRight.SetAsLastSibling();
        rectMid.SetAsLastSibling();

        rectLeft.DOKill();
        rectMid.DOKill();
        rectRight.DOKill();

        Sequence seq = DOTween.Sequence();
        activeMatchSequences.Add(seq);

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
            activeMatchSequences.Remove(seq);

            // The board was reset while this animation was running.
            if (capturedVersion != boardStateVersion)
                return;

            SpawnDestroyParticle(middleTile);
            SpawnSlotGlow(glowWorldPos);

            foreach (GameObject tile in matchedTiles)
            {
                if (tile == null)
                    continue;

                MatchBoard.instance.RemoveTile(tile);
                MatchBoard.instance.ForgetTrayTile(tile);

                ObjectPoolManager.Instance.Despawn(tile);
            }

            Tween delayedCall = null;

            delayedCall = DOVirtual.DelayedCall(0.2f, () =>
            {
                activeDelayedCalls.Remove(delayedCall);

                if (capturedVersion != boardStateVersion)
                    return;

                if (MatchBoard.instance != null)
                {
                    MatchBoard.instance.RearrangeBoard();
                }

                activePopAnimation = Mathf.Max(0, activePopAnimation - 1);
                TryCheckLevelComplete();
            });

            activeDelayedCalls.Add(delayedCall);
        });

        return true;
    }

    void Rearrange()
    {
        MatchBoard.instance.RearrangeBoard();
    }

    public void ResetBoardState()
    {
        boardStateVersion++;

        foreach (Sequence seq in activeMatchSequences)
        {
            if (seq != null && seq.IsActive())
            {
                seq.Kill(false);
            }
        }

        activeMatchSequences.Clear();

        foreach (Tween delayedCall in activeDelayedCalls)
        {
            if (delayedCall != null && delayedCall.IsActive())
            {
                delayedCall.Kill(false);
            }
        }

        activeDelayedCalls.Clear();

        foreach (Sequence glowSequence in activeGlowSequences)
        {
            if (glowSequence != null && glowSequence.IsActive())
            {
                glowSequence.Kill(false);
            }
        }

        activeGlowSequences.Clear();

        foreach (GameObject glow in activeGlowObjects)
        {
            if (glow != null)
            {
                Destroy(glow);
            }
        }

        activeGlowObjects.Clear();

        foreach (GameObject particle in activeParticleObjects)
        {
            if (particle != null)
            {
                Destroy(particle);
            }
        }

        activeParticleObjects.Clear();

        StopAllCoroutines();

        activePopAnimation = 0;
        activeDestroyParticles = 0;

        CancelInvoke(nameof(Rearrange));
    }

    public void CheckLevelComplete()
    {
        if (MatchBoard.instance == null || BoardSpawner.instance == null)
        {
            return;
        }

        MatchBoard.instance.CleanBoard();

        Transform tileParent = BoardSpawner.instance.GetTileParent();

        if (tileParent == null)
        {
            Debug.LogWarning("MatchBoardMatch: Tile parent is null.");
            return;
        }

        int boardTiles = 0;

        foreach (Transform child in tileParent)
        {
            if (child == null)
                continue;

            if (!child.gameObject.activeSelf)
                continue;

            Tile tile = child.GetComponent<Tile>();

            if (tile == null)
                continue;

            if (!tile.IsMoved())
                boardTiles++;
        }

        int matchTiles = MatchBoard.instance.GetTileCount();

        if (boardTiles <= 0 && matchTiles <= 0)
        {
            if (GameManager.instance != null && GameManager.instance.isGameInProgress)
            {
                GameManager.instance.LevelComplete();
            }
        }
    }

    private void CancelMatchedTiles(List<GameObject> matchedTiles)
    {
        foreach (GameObject tileObj in matchedTiles)
        {
            if (tileObj == null)
                continue;

            Tile tile = tileObj.GetComponent<Tile>();

            if (tile != null)
            {
                tile.isMatched = false;
            }
        }

        activePopAnimation = Mathf.Max(0, activePopAnimation - 1);
    }

    IEnumerator WaitForParticleFinish(float duration, int capturedVersion, GameObject particle)
    {
        yield return new WaitForSecondsRealtime(duration);

        activeParticleObjects.Remove(particle);

        if (capturedVersion != boardStateVersion)
            yield break;

        activeDestroyParticles = Mathf.Max(0, activeDestroyParticles - 1);
        TryCheckLevelComplete();
    }

    public void PlayDestroyEffect(GameObject tile)
    {
        if (tile == null)
            return;

        SpawnDestroyParticle(tile);
        if (MatchBoard.instance != null)
        {
            MatchBoard.instance.ForgetTrayTile(tile);
        }
        
        ObjectPoolManager.Instance.Despawn(tile);
    }

    void SpawnDestroyParticle(GameObject tileObj)
    {
        if (tileObj == null)
            return;

        if (destroyParticle == null)
        {
            Debug.LogWarning("MatchBoardMatch: Destroy Particle prefab is not assigned.");
            return;
        }

        RectTransform tileRect = tileObj.transform as RectTransform;

        if (tileRect == null)
        {
            Debug.LogWarning($"MatchBoardMatch: {tileObj.name} has no RectTransform.");
            return;
        }

        Vector3 position = tileRect.position;

        GameObject particle = Instantiate(destroyParticle, position, Quaternion.identity, particleParent);
        activeParticleObjects.Add(particle);

        ParticleSystem ps = particle.GetComponent<ParticleSystem>();

        if (ps == null)
        {
            Debug.LogWarning("MatchBoardMatch: Destroy Particle prefab has no ParticleSystem.");

            activeParticleObjects.Remove(particle);
            Destroy(particle);
            return;
        }
        activeDestroyParticles++;

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
        int capturedVersion = boardStateVersion;

        StartCoroutine(WaitForParticleFinish(particleDuration, capturedVersion, particle));
    }

    void SpawnSlotGlow(Vector3 worldPos)
    {
        if (slotGlowPrefab == null)
            return;

        int capturedVersion = boardStateVersion;

        GameObject glow = Instantiate(slotGlowPrefab, particleParent
        );

        activeGlowObjects.Add(glow);

        RectTransform rect = glow.transform as RectTransform;

        if (rect == null)
        {
            activeGlowObjects.Remove(glow);
            Destroy(glow);
            return;
        }

        rect.position = worldPos;
        rect.localScale = Vector3.one * 0.5f;

        //ForceTopLayer(glow, 30001);
        rect.SetAsLastSibling();

        CanvasGroup cg = glow.GetComponent<CanvasGroup>();

        if (cg == null)
        {
            cg = glow.AddComponent<CanvasGroup>();
        }

        cg.alpha = 1f;
        Sequence seq = DOTween.Sequence();
        activeGlowSequences.Add(seq);

        seq.Append(rect.DOScale(Vector3.one * 1.6f, 0.2f).SetEase(Ease.OutCubic));
        seq.Join(cg.DOFade(0f, 0.2f));

        seq.OnComplete(() =>
        {
            activeGlowSequences.Remove(seq);
            activeGlowObjects.Remove(glow);

            if (capturedVersion != boardStateVersion)
            {
                if (glow != null)
                {
                    Destroy(glow);
                }
                return;
            }

            if (glow != null)
            {
                Destroy(glow);
            }
        });
    }

    ParticleSystem.MinMaxGradient GetMultiColor(Color c1, Color c2)
    {
        Gradient gradient = new Gradient();

        gradient.SetKeys(
            new GradientColorKey[]
            {
            new GradientColorKey(c1, 0f),
            new GradientColorKey(c2, 1f)
            },
            new GradientAlphaKey[]
            {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
            });

        return new ParticleSystem.MinMaxGradient
        {
            mode = ParticleSystemGradientMode.RandomColor,
            gradient = gradient
        };
    }

    ParticleSystem.MinMaxGradient GetMultiColor(Color c1, Color c2, Color c3)
    {
        Gradient gradient = new Gradient();

        gradient.SetKeys(
            new GradientColorKey[]
            {
            new GradientColorKey(c1, 0f),
            new GradientColorKey(c2, 0.5f),
            new GradientColorKey(c3, 1f)
            },
            new GradientAlphaKey[]
            {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
            });

        return new ParticleSystem.MinMaxGradient
        {
            mode = ParticleSystemGradientMode.RandomColor,
            gradient = gradient
        };
    }

    private void TryCheckLevelComplete()
    {
        if (activePopAnimation <= 0 &&
            activeDestroyParticles <= 0)
        {
            CheckLevelComplete();
        }
    }
}