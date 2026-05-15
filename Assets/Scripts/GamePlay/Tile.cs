using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    private bool isMoved = false;
    public int tileId;
    public int row;
    public int col;
    public int layer;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isMoved)
            return;

        if (IsBlocked())
            return;

        MoveToBoard();
    }

    public bool IsBlocked()
    {
        Transform parent = transform.parent;

        RectTransform myRect = GetComponent<RectTransform>();

        foreach (Transform child in parent)
        {
            if (child == transform)
                continue;

            Tile other = child.GetComponent<Tile>();

            if (other == null)
                continue;

            if (other.layer <= this.layer)
                continue;

            RectTransform otherRect = other.GetComponent<RectTransform>();

            float distance = Vector2.Distance(
                myRect.anchoredPosition,
                otherRect.anchoredPosition
            );

            if (distance < 120f)
            {
                return true;
            }
        }
        return false;
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
        StartCoroutine(ClickAnimation());
        bool added = MatchBoard.instance.AddTile(gameObject);

        if (added)
        {
            isMoved = true;
            SoundManager.instance.PlaySound(SoundName.TileClick);
        }
    }

    public bool IsMoved()
    {
        return isMoved;
    }

    IEnumerator ClickAnimation()
    {
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 originalScale = transform.localScale;

        Vector3 pressedScale = originalScale * 0.9f;

        float time = 0;
        float duration = 0.08f;

        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, pressedScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;

        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(pressedScale, originalScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }
}