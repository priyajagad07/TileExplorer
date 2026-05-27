using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;


public class Tile : MonoBehaviour, IPointerClickHandler
{
    private bool isMoved = false;
    public int tileId;
    public int row;
    public int col;
    public int layer;
    private Image[] tileImages;

    void Awake()
    {
        tileImages = GetComponentsInChildren<Image>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isMoved)
            return;

        if (IsBlocked())
        {
            PlayBlockedFeedback();
            return;
        }

        MoveToBoard();
    }

    public bool IsBlocked()
    {
        if (isMoved)
            return false;

        Transform parent = transform.parent;
        RectTransform myRect = GetComponent<RectTransform>();

        Rect myLocalRect = GetLocalRect(myRect);

        foreach (Transform child in parent)
        {
            if (child == transform)
                continue;

            Tile other = child.GetComponent<Tile>();

            if (other == null)
                continue;

            if (other.IsMoved())
                continue;

            if (other.layer <= this.layer)
                continue;

            RectTransform otherRect = other.GetComponent<RectTransform>();
            Rect otherLocalRect = GetLocalRect(otherRect);

            if (myLocalRect.Overlaps(otherLocalRect))
            {
                return true;
            }
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

    public void SetMoved(bool value)
    {
        isMoved = value;
    }

    public void MoveToBoard()
    {
        if (isMoved)
            return;

        BoosterSystem.instance.RecordMove(gameObject);
        PlayClickAnimation();
        bool added = MatchBoard.instance.AddTile(gameObject);

        if (added)
        {
            isMoved = true;

            RefreshVisual();
            RefreshAllTiles();

            SoundManager.instance.PlaySound(SoundName.TileClick);
        }
    }

    public bool IsMoved()
    {
        return isMoved;
    }

    void RefreshAllTiles()
    {
        Tile[] allTiles = transform.parent.GetComponentsInChildren<Tile>();

        foreach (Tile tile in allTiles)
        {
            tile.RefreshVisual();
        }
    }

    void PlayClickAnimation()
    {
        transform.DOKill();

        transform
            .DOScale(
                transform.localScale * 0.88f,
                0.08f
            )
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutQuad);
    }

    public void RefreshVisual()
    {
        bool blocked = IsBlocked();

        Color targetColor = blocked ? new Color(0.45f, 0.45f, 0.45f, 1f) : Color.white;

        foreach (Image img in tileImages)
        {
            img.DOKill();
            img.DOColor(targetColor, 0.2f);
        }
    }

    void PlayBlockedFeedback()
    {
        transform.DOKill();

        transform
            .DOPunchScale(Vector3.one * 0.05f, 0.2f, 5, 0.5f);

        transform
            .DOPunchPosition(
                Vector3.right * 8f, 0.2f, 8, 0.5f);

        SoundManager.instance.PlaySound(SoundName.TileBlocked);
    }

    public static void RefreshAllTileVisuals(Transform parent)
    {
        Tile[] allTiles =
            parent.GetComponentsInChildren<Tile>();

        foreach (Tile tile in allTiles)
        {
            tile.RefreshVisual();
        }
    }
}