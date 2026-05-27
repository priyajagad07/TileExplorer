using UnityEngine;
using DG.Tweening;

public class MatchBoardMovement : MonoBehaviour
{
    public void MoveTile(GameObject tile, Transform targetSlot)
    {
        if (tile == null)
            return;

        RectTransform rect =
            tile.GetComponent<RectTransform>();

        rect.DOKill();

        rect.DOMove(
            targetSlot.position,
            0.15f
        )
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            if (tile == null)
                return;

            rect.position =
                targetSlot.position;

            tile.transform.SetParent(targetSlot);

            tile.transform.SetAsLastSibling();

            rect.anchoredPosition =
                Vector2.zero;
        });
    }
}