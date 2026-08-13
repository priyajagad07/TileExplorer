using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using Solo.MOST_IN_ONE;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    private bool isMoved = false;
    public int tileId;
    public int row;
    public int col;
    public int layer;
    private Image[] tileImages;
    public bool isMatched = false;

    [Header("Explosion Colors")]
    public Color[] particleColors;

    [Header("Jelly Mechanic")]
    public bool isJellyLocked = false;
    public int jellyHealth = 0;
    public GameObject jellyOverlayPrefab;
    private CanvasGroup activeJellyOverlay;
    public GameObject jellySplashPrefab;
    private Text jellyText;

    private Vector3 originalScale;
    private RectTransform rect;
    private readonly Vector2 boardSize = new Vector2(140, 140);
    private Vector2 spawnSize;
    private RectTransform tileImage;
    private Vector2 imageSpawnSize;
    private readonly Vector2 boardImageSize = new Vector2(105, 105);
    private Sequence jellyUnlockSequence;
    private GameObject activeJellySplash;

    void Awake()
    {
        rect = GetComponent<RectTransform>();

        if (transform.childCount > 0)
        {
            tileImage =
                transform.GetChild(0)
                    .GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError(
                $"Tile {gameObject.name} has no child image."
            );
        }

        tileImages =
            GetComponentsInChildren<Image>();

        originalScale =
            Vector3.one * 0.9f;
    }

    public void CacheSpawnSize()
    {
        spawnSize = rect.sizeDelta;

        if (tileImage != null)
            imageSpawnSize = tileImage.sizeDelta;
    }

    public void AnimateToBoardSize()
    {
        rect.DOSizeDelta(boardSize, 0.18f).SetEase(Ease.OutQuad);

        if (tileImage != null)
        {
            tileImage.DOSizeDelta(boardImageSize, 0.18f).SetEase(Ease.OutQuad);
        }
    }

    public void AnimateToSpawnSize()
    {
        rect.DOSizeDelta(spawnSize, 0.18f).SetEase(Ease.OutQuad);

        if (tileImage != null)
        {
            tileImage.DOSizeDelta(imageSpawnSize, 0.18f).SetEase(Ease.OutQuad);
        }
    }

    public void MakeJelly(int health = 10)
    {
        isJellyLocked = true;
        jellyHealth = health;

        if (jellyOverlayPrefab != null && activeJellyOverlay == null)
        {
            GameObject newOverlay = Instantiate(jellyOverlayPrefab, transform);

            // CHANGED: Force the overlay to stretch perfectly over the tile's exact size!
            RectTransform overlayRect = newOverlay.GetComponent<RectTransform>();
            if (overlayRect != null)
            {
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;
                overlayRect.localScale = Vector3.one;
            }

            activeJellyOverlay = newOverlay.GetComponent<CanvasGroup>();
            if (activeJellyOverlay == null)
            {
                activeJellyOverlay = newOverlay.AddComponent<CanvasGroup>();
            }

            activeJellyOverlay.alpha = 1f;

            // NEW: Dynamically create the Countdown Text
            GameObject textObj = new GameObject("JellyCountText");
            textObj.transform.SetParent(newOverlay.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            jellyText = textObj.AddComponent<Text>();
            jellyText.text = jellyHealth.ToString();
            jellyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            jellyText.fontSize = 65;
            jellyText.alignment = TextAnchor.MiddleCenter;
            jellyText.color = Color.white;
            jellyText.fontStyle = FontStyle.Bold;

            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.5f);
            outline.effectDistance = new Vector2(3, -3);
        }

        if (activeJellyOverlay != null)
        {
            activeJellyOverlay.gameObject.SetActive(true);
            activeJellyOverlay.alpha = 1f; // No transparency!
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (BoardSpawner.instance != null && BoardSpawner.instance.isSpawning) return;
        if (MatchBoard.instance == null)
            return;

        if (MatchBoard.instance.isInputLocked)
            return;

        if (isMoved) return;

        if (TutorialManager.instance != null &&
            !TutorialManager.instance.IsTileClickAllowed(
                gameObject))
        {
            return;
        }

        // Soft Undo tutorial: target tile continues the flow;
        // any other tile dismisses the free tutorial use.
        if (UndoTutorialManager.instance != null &&
            UndoTutorialManager.instance.IsRunning)
        {
            UndoTutorialManager.instance
                .HandleTileClick(gameObject);
        }

        if (IsBlocked())
        {
            PlayBlockedFeedback();
            return;
        }

        // Clicking jelly no longer damages it — just the squish.
        if (isJellyLocked)
        {
            PlayJellySquishFeedback();
            return;
        }

        if (IdleHintManager.instance != null)
        {
            bool tutorialRunning =
                (
                    TutorialManager.instance != null &&
                    TutorialManager.instance
                        .IsAnyTutorialActive
                )
                ||
                (
                    UndoTutorialManager.instance != null &&
                    UndoTutorialManager.instance
                        .IsRunning
                );

            if (!tutorialRunning)
            {
                IdleHintManager.instance.ResetIdleTimer();
            }
            else
            {
                IdleHintManager.instance.StopHints();
            }
        }

        MoveToBoard();
    }

    public void TakeJellyDamage()
    {
        if (!isJellyLocked || jellyHealth <= 0)
            return;

        jellyHealth =
            Mathf.Max(0, jellyHealth - 1);

        if (jellyText != null)
        {
            jellyText.text = jellyHealth.ToString();
        }

        if (activeJellyOverlay != null)
        {
            activeJellyOverlay.transform.DOKill(true);
            activeJellyOverlay.transform.localScale = Vector3.one;
            activeJellyOverlay.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5, 0.5f);
        }

        if (jellyHealth <= 0)
        {
            UnlockJelly();
        }
    }

    /// <summary>
    /// Force-clears jelly when the board would otherwise soft-lock.
    /// </summary>
    public void ForceUnlockJelly()
    {
        if (!isJellyLocked)
            return;

        jellyHealth = 0;

        if (jellyText != null)
        {
            jellyText.text = "0";
        }

        UnlockJelly();
    }

    void UnlockJelly()
    {
        isJellyLocked = false;

        // Hide the text instantly before the pop animation
        if (jellyText != null) jellyText.gameObject.SetActive(false);

        if (activeJellyOverlay != null)
        {
            activeJellyOverlay.DOKill(true);
            activeJellyOverlay.transform.DOKill(true);

            jellyUnlockSequence?.Kill(false);

            Sequence premiumPop = DOTween.Sequence();
            jellyUnlockSequence = premiumPop;
            premiumPop.Append(activeJellyOverlay.transform.DOScale(Vector3.one * 0.7f, 0.1f).SetEase(Ease.InOutQuad));
            premiumPop.Join(activeJellyOverlay.transform.DOPunchRotation(new Vector3(0, 0, 15f), 0.15f, 1));
            premiumPop.Append(activeJellyOverlay.transform.DOScale(new Vector3(1f, 1f, 1f), 0.15f).SetEase(Ease.OutBack));

            premiumPop.InsertCallback(0.25f, () =>
            {
                if (jellySplashPrefab != null)
                {
                    if (activeJellySplash != null)
                    {
                        Destroy(activeJellySplash);
                    }

                    activeJellySplash = Instantiate(
                        jellySplashPrefab,
                        transform.position,
                        Quaternion.identity,
                        transform.parent
                    );
                }
            });

            premiumPop.Append(activeJellyOverlay.transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack));
            premiumPop.Join(activeJellyOverlay.DOFade(0f, 0.1f).SetDelay(0.05f));

            CanvasGroup overlayToDestroy = activeJellyOverlay;

            premiumPop.OnComplete(() =>
            {
                jellyUnlockSequence = null;
                if (overlayToDestroy != null)
                {
                    Destroy(overlayToDestroy.gameObject);
                }

                if (activeJellyOverlay == overlayToDestroy)
                {
                    activeJellyOverlay = null;
                    jellyText = null;
                }
            });
        }
        else
        {
            if (jellySplashPrefab != null)
            {
                if (activeJellySplash != null)
                {
                    Destroy(activeJellySplash);
                }

                activeJellySplash = Instantiate(
                    jellySplashPrefab,
                    transform.position,
                    Quaternion.identity,
                    transform.parent
                );
            }
        }
    }

    public bool IsBlocked()
    {
        if (isMoved)
            return false;

        Transform parent = transform.parent;

        if (parent == null)
            return false;

        RectTransform myRect = rect;

        if (myRect == null)
            return false;

        Rect myLocalRect = GetLocalRect(myRect);

        foreach (Transform child in parent)
        {
            if (child == transform) continue;

            if (!child.gameObject.activeSelf) continue;

            Tile other = child.GetComponent<Tile>();
            if (other == null || other.IsMoved() || other.layer <= this.layer) continue;

            // Tile UI objects use RectTransform as their transform;
            // prefer the Awake-cached rect when available (same component).
            RectTransform otherRect = other.rect != null
                ? other.rect
                : child as RectTransform;

            if (otherRect == null)
                continue;

            Rect otherLocalRect = GetLocalRect(otherRect);

            if (myLocalRect.Overlaps(otherLocalRect)) return true;
        }
        return false;
    }

    Rect GetLocalRect(RectTransform rect)
    {
        Vector2 centerPos = rect.anchoredPosition;
        float width = rect.rect.width * rect.localScale.x;
        float height = rect.rect.height * rect.localScale.y;

        return new Rect(
            centerPos.x - (width / 2f),
            centerPos.y - (height / 2f),
            width,
            height
        );
    }

    public void SetMoved(bool value) { isMoved = value; }

    public void MoveToBoard()
    {
        if (isMoved)
            return;

        if (MatchBoard.instance == null)
            return;
        // Jelly itself can never enter the MatchBoard.
        if (isJellyLocked)
        {
            PlayJellySquishFeedback();
            return;
        }

        if (MatchBoard.instance.GetPlacedTiles().Count >=
            MatchBoard.instance.slots.Count)
        {
            PlayBlockedFeedback();
            return;
        }

        if (UndoTutorialManager.instance != null)
        {
            UndoTutorialManager.instance
                .PrepareTargetTileMove(
                    gameObject
                );
        }

        // Save the main board parent BEFORE the tile starts moving.
        Transform boardParent = transform.parent;

        // Record the exact board state BEFORE
        // any size or scale animation begins.
        UndoData pendingUndoData = null;

        if (BoosterSystem.instance != null)
        {
            pendingUndoData =
                BoosterSystem.instance.CreateUndoData(
                    gameObject
                );
        }

        AnimateToBoardSize();
        PlayClickAnimation();

        bool added = MatchBoard.instance.AddTile(gameObject);

        if (added)
        {
            if (BoosterSystem.instance != null &&
                pendingUndoData != null)
            {
                BoosterSystem.instance.CommitUndoData(
                    pendingUndoData
                );
            }

            isMoved = true;

            RefreshVisual();

            if (UndoTutorialManager.instance != null)
            {
                UndoTutorialManager.instance
                    .OnTileMovedToTray(
                        gameObject
                    );
            }

            // Single hierarchy scan: refresh visuals, then apply jelly damage.
            // Order matches the previous two-pass GetComponentsInChildren flow.
            Tile[] allTiles =
                boardParent.GetComponentsInChildren<Tile>(false);

            for (int i = 0; i < allTiles.Length; i++)
            {
                Tile tile = allTiles[i];
                if (tile != null)
                {
                    tile.RefreshVisual();
                }
            }

            // Every successful tile entering the MatchBoard
            // damages currently active Jelly tiles by 1.
            for (int i = 0; i < allTiles.Length; i++)
            {
                Tile t = allTiles[i];
                if (t == null)
                    continue;

                if (t.isJellyLocked &&
                    !t.IsMoved() &&
                    !t.IsBlocked())
                {
                    t.TakeJellyDamage();
                }
            }

            if (AutoShuffleManager.instance != null)
            {
                AutoShuffleManager.instance
                    .ResolveJellySoftLockIfNeeded();
            }

            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlayHaptic(
                    MOST_HapticFeedback.HapticTypes.LightImpact
                );

                SoundManager.instance.PlayTileClick(
                    this.tileId
                );
            }
        }
        else
        {
            // AddTile failed, so restore the tile.
            transform.DOKill();

            if (rect != null)
            {
                rect.DOKill();
            }

            transform.localScale = originalScale;

            AnimateToSpawnSize();
        }
    }

    void PlayJellySquishFeedback()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayHaptic(
                MOST_HapticFeedback.HapticTypes.SoftImpact
            );
        }

        transform.DOKill(true);
        transform.localScale = originalScale;

        Sequence jellySquish = DOTween.Sequence();

        jellySquish.Append(
            transform.DOScale(
                new Vector3(
                    originalScale.x * 1.15f,
                    originalScale.y * 0.85f,
                    originalScale.z
                ),
                0.15f
            ).SetEase(Ease.OutQuad)
        );

        jellySquish.Append(
            transform.DOScale(
                originalScale,
                0.4f
            ).SetEase(Ease.OutElastic)
        );

        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlaySound(
                SoundName.TileBlocked
            );
        }
    }

    public bool IsMoved() { return isMoved; }

    void PlayClickAnimation()
    {
        transform.DOKill(true);
        transform.DOScale(originalScale * 0.88f, 0.08f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
    }

    public void RefreshVisual()
    {
        if (tileImages == null)
            return;

        bool blocked = IsBlocked();
        Color targetColor = blocked ? new Color(0.65f, 0.65f, 0.65f, 1f) : Color.white;

        for (int i = 0; i < tileImages.Length; i++)
        {
            Image img = tileImages[i];
            if (img == null)
                continue;

            img.DOKill();
            img.DOColor(targetColor, 0.2f);
        }
    }

    void PlayBlockedFeedback()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayHaptic(
                MOST_HapticFeedback.HapticTypes.LightImpact
            );
        }

        transform.DOKill(true);
        transform.localScale = originalScale;

        transform.DOPunchScale(
            Vector3.one * 0.05f,
            0.2f,
            5,
            0.5f
        );

        transform.DOPunchPosition(
            Vector3.right * 8f,
            0.2f,
            8,
            0.5f
        );

        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlaySound(
                SoundName.TileBlocked
            );
        }
    }

    public static void RefreshAllTileVisuals(Transform parent)
    {
        if (parent == null)
            return;

        Tile[] allTiles =
            parent.GetComponentsInChildren<Tile>(false);

        foreach (Tile tile in allTiles)
        {
            if (tile != null)
            {
                tile.RefreshVisual();
            }
        }
    }
    public void ResetTileState()
    {
        if (activeJellySplash != null)
        {
            Destroy(activeJellySplash);
            activeJellySplash = null;
        }

        jellyUnlockSequence?.Kill(false);
        jellyUnlockSequence = null;
        // -----------------------------------------------
        // 1. KILL ALL OLD TWEENS
        // -----------------------------------------------

        transform.DOKill();

        if (rect != null)
        {
            rect.DOKill();
        }

        if (tileImage != null)
        {
            tileImage.DOKill();
        }

        if (tileImages != null)
        {
            foreach (Image img in tileImages)
            {
                if (img != null)
                {
                    img.DOKill();
                }
            }
        }

        if (activeJellyOverlay != null)
        {
            activeJellyOverlay.DOKill();
            activeJellyOverlay.transform.DOKill();
        }

        // -----------------------------------------------
        // 2. RESET GAMEPLAY STATE
        // -----------------------------------------------

        isMatched = false;
        isMoved = false;

        isJellyLocked = false;
        jellyHealth = 0;

        // -----------------------------------------------
        // 3. RESET TRANSFORM
        // -----------------------------------------------

        transform.localScale = originalScale;
        transform.localRotation = Quaternion.identity;

        // -----------------------------------------------
        // 4. RESTORE ORIGINAL BOARD SIZE
        // -----------------------------------------------

        if (rect != null)
        {
            rect.localScale = originalScale;
            rect.localRotation = Quaternion.identity;

            if (spawnSize != Vector2.zero)
            {
                rect.sizeDelta = spawnSize;
            }
        }

        if (tileImage != null &&
            imageSpawnSize != Vector2.zero)
        {
            tileImage.sizeDelta = imageSpawnSize;
        }

        // -----------------------------------------------
        // 5. REMOVE OLD JELLY OBJECTS
        // -----------------------------------------------

        if (activeJellyOverlay != null)
        {
            Destroy(activeJellyOverlay.gameObject);
            activeJellyOverlay = null;
        }

        // jellyText is normally a child of the overlay,
        // so do not destroy it again separately.
        jellyText = null;

        // -----------------------------------------------
        // 6. RESET TEMPORARY TUTORIAL / SORTING COMPONENTS
        // -----------------------------------------------

        GraphicRaycaster raycaster =
            GetComponent<GraphicRaycaster>();

        if (raycaster != null)
        {
            Destroy(raycaster);
        }

        Canvas canvas = GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.overrideSorting = false;
            canvas.sortingOrder = 0;

            Destroy(canvas);
        }

        // -----------------------------------------------
        // 7. RESTORE VISUAL STATE
        // -----------------------------------------------

        if (tileImages != null)
        {
            foreach (Image img in tileImages)
            {
                if (img != null)
                {
                    img.color = Color.white;
                }
            }
        }
    }
    public Vector2 GetCurrentSize()
    {
        return rect.sizeDelta;
    }

    public Vector2 GetCurrentImageSize()
    {
        if (tileImage != null)
            return tileImage.sizeDelta;

        return Vector2.zero;
    }

    public void RestoreExactBoardSize(
        Vector3 savedScale,
        Vector2 savedSize,
        Vector2 savedImageSize)
    {
        rect.DOKill();

        if (tileImage != null)
        {
            tileImage.DOKill();
        }

        rect.localScale = savedScale;
        rect.sizeDelta = savedSize;

        if (tileImage != null)
        {
            tileImage.sizeDelta = savedImageSize;
        }
    }
}