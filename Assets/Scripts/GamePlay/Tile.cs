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

    // NEW: Reference to the dynamic text
    private Text jellyText;

    private Vector3 originalScale;
    private RectTransform rect;
    private readonly Vector2 boardSize = new Vector2(140, 140);
    private Vector2 spawnSize;
    private RectTransform tileImage;
    private Vector2 imageSpawnSize;
    private readonly Vector2 boardImageSize = new Vector2(105, 105);

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        tileImage = transform.GetChild(0).GetComponent<RectTransform>();
        tileImages = GetComponentsInChildren<Image>();
        originalScale = Vector3.one * 0.9f;
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
        if (MatchBoard.instance.isInputLocked) return;
        if (isMoved) return;

        if (TutorialManager.instance != null && !TutorialManager.instance.IsTileClickAllowed(this.gameObject))
            return;

        if (IsBlocked())
        {
            PlayBlockedFeedback();
            return;
        }

        // THE FIX 1: Clicking the jelly no longer damages it. It just plays the squish!
        if (isJellyLocked)
        {
            PlayJellySquishFeedback();
            return;
        }

        if (IdleHintManager.instance != null) IdleHintManager.instance.ResetIdleTimer();
        MoveToBoard();
    }

    public void TakeJellyDamage()
    {
        if (!isJellyLocked) return;

        jellyHealth--;

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

    void UnlockJelly()
    {
        isJellyLocked = false;

        // Hide the text instantly before the pop animation
        if (jellyText != null) jellyText.gameObject.SetActive(false);

        if (activeJellyOverlay != null)
        {
            activeJellyOverlay.DOKill(true);
            activeJellyOverlay.transform.DOKill(true);

            Sequence premiumPop = DOTween.Sequence();
            premiumPop.Append(activeJellyOverlay.transform.DOScale(Vector3.one * 0.7f, 0.1f).SetEase(Ease.InOutQuad));
            premiumPop.Join(activeJellyOverlay.transform.DOPunchRotation(new Vector3(0, 0, 15f), 0.15f, 1));
            premiumPop.Append(activeJellyOverlay.transform.DOScale(new Vector3(1f, 1f, 1f), 0.15f).SetEase(Ease.OutBack));

            premiumPop.InsertCallback(0.25f, () =>
            {
                if (jellySplashPrefab != null)
                {
                    Instantiate(jellySplashPrefab, transform.position, Quaternion.identity, transform.parent);
                }
            });

            premiumPop.Append(activeJellyOverlay.transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack));
            premiumPop.Join(activeJellyOverlay.DOFade(0f, 0.1f).SetDelay(0.05f));

            premiumPop.OnComplete(() =>
            {
                Destroy(activeJellyOverlay.gameObject);
            });
        }
        else
        {
            if (jellySplashPrefab != null)
            {
                Instantiate(jellySplashPrefab, transform.position, Quaternion.identity, transform.parent);
            }
        }
    }

    public bool IsBlocked()
    {
        if (isMoved) return false;

        Transform parent = transform.parent;
        RectTransform myRect = GetComponent<RectTransform>();
        Rect myLocalRect = GetLocalRect(myRect);

        foreach (Transform child in parent)
        {
            if (child == transform) continue;

            if (!child.gameObject.activeSelf) continue;

            Tile other = child.GetComponent<Tile>();
            if (other == null || other.IsMoved() || other.layer <= this.layer) continue;

            RectTransform otherRect = other.GetComponent<RectTransform>();
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
        if (isMoved) return;
        if (MatchBoard.instance.GetPlacedTiles().Count >= MatchBoard.instance.slots.Count)
        {
            PlayBlockedFeedback();
            return;
        }

        AnimateToBoardSize();
        PlayClickAnimation();

        bool added = MatchBoard.instance.AddTile(gameObject);

        if (added)
        {
            isMoved = true;

            RefreshVisual();
            RefreshAllTiles();

            Tile[] allTiles = transform.parent.GetComponentsInChildren<Tile>(false);
            foreach (Tile t in allTiles)
            {
                if (t.isJellyLocked && !t.IsMoved() && !t.IsBlocked())
                {
                    t.TakeJellyDamage();
                }
            }

            SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.LightImpact);
            SoundManager.instance.PlayTileClick(this.tileId);
        }
    }

    void PlayJellySquishFeedback()
    {
        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.SoftImpact);

        transform.DOKill(true);
        transform.localScale = originalScale;

        Sequence jellySquish = DOTween.Sequence();
        jellySquish.Append(transform.DOScale(new Vector3(originalScale.x * 1.15f, originalScale.y * 0.85f, originalScale.z), 0.15f).SetEase(Ease.OutQuad));
        jellySquish.Append(transform.DOScale(originalScale, 0.4f).SetEase(Ease.OutElastic));

        SoundManager.instance.PlaySound(SoundName.TileBlocked);
    }

    public bool IsMoved() { return isMoved; }

    void RefreshAllTiles()
    {
        Tile[] allTiles = transform.parent.GetComponentsInChildren<Tile>();
        foreach (Tile tile in allTiles) tile.RefreshVisual();
    }

    void PlayClickAnimation()
    {
        transform.DOKill(true);
        transform.DOScale(originalScale * 0.88f, 0.08f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
    }

    public void RefreshVisual()
    {
        bool blocked = IsBlocked();
        Color targetColor = blocked ? new Color(0.65f, 0.65f, 0.65f, 1f) : Color.white;

        foreach (Image img in tileImages)
        {
            img.DOKill();
            img.DOColor(targetColor, 0.2f);
        }
    }

    void PlayBlockedFeedback()
    {
        SoundManager.instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.LightImpact);
        transform.DOKill(true);
        transform.localScale = originalScale;
        transform.DOPunchScale(Vector3.one * 0.05f, 0.2f, 5, 0.5f);
        transform.DOPunchPosition(Vector3.right * 8f, 0.2f, 8, 0.5f);
        SoundManager.instance.PlaySound(SoundName.TileBlocked);
    }

    public static void RefreshAllTileVisuals(Transform parent)
    {
        Tile[] allTiles = parent.GetComponentsInChildren<Tile>();
        foreach (Tile tile in allTiles) tile.RefreshVisual();
    }

    public void ResetTileState()
    {
        isMoved = false;
        isMatched = false;
        isJellyLocked = false;
        jellyHealth = 0;

        transform.DOKill(true);

        UnityEngine.UI.GraphicRaycaster raycaster = GetComponent<UnityEngine.UI.GraphicRaycaster>();
        if (raycaster != null)
        {
            Destroy(raycaster);
        }

        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            Destroy(canvas);
        }
        // -----------------------------------------------

        RectTransform rectComponent = GetComponent<RectTransform>();
        rectComponent.localScale = originalScale;
        rectComponent.localRotation = Quaternion.identity;

        if (spawnSize != Vector2.zero)
        {
            rectComponent.sizeDelta = spawnSize;
        }
        if (tileImage != null && imageSpawnSize != Vector2.zero)
        {
            tileImage.sizeDelta = imageSpawnSize;
        }

        if (activeJellyOverlay != null)
        {
            Destroy(activeJellyOverlay.gameObject);
            activeJellyOverlay = null;
        }

        if (jellyText != null)
        {
            Destroy(jellyText.gameObject);
            jellyText = null;
        }

        RefreshVisual();
    }
}