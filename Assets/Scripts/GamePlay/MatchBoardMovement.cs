using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
public class MatchBoardMovement : MonoBehaviour
{
    private HashSet<GameObject> knownTrayTiles = new HashSet<GameObject>();

    public void MoveTile(GameObject tile, Transform targetSlot)
    {
        if (tile == null) return;


        knownTrayTiles.RemoveWhere(t => t == null);

        RectTransform rect = tile.GetComponent<RectTransform>();

        rect.DOKill();

        // Check our memory: Is this the very first time this tile is entering the tray?
        bool isFirstTimeArrival = !knownTrayTiles.Contains(tile);

        if (isFirstTimeArrival)
        {
            // Add it to memory so it NEVER fluffs again
            knownTrayTiles.Add(tile);
        }

        rect.DOMove(targetSlot.position, 0.15f)
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            if (tile == null || rect == null) return;

            rect.position = targetSlot.position;
            tile.transform.SetParent(targetSlot);
            tile.transform.SetAsLastSibling();
            rect.anchoredPosition = Vector2.zero;

            if (isFirstTimeArrival)
            {
                string fluffId = tile.GetInstanceID() + "_landingFluff";
                DOTween.Kill(fluffId);

                Sequence fluffSeq = DOTween.Sequence().SetId(fluffId);

                // Squish down
                fluffSeq.Append(rect.DOScale(new Vector3(0.95f, 0.75f, 1f), 0.08f).SetEase(Ease.OutQuad));
                // Bounce back
                fluffSeq.Append(rect.DOScale(Vector3.one * 0.9f, 0.15f).SetEase(Ease.OutBack));
            }
            else
            {
                rect.DOScale(Vector3.one * 0.9f, 0.1f);
            }
        });
    }
}