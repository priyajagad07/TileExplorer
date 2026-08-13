using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class MatchBoardMovement : MonoBehaviour
{
    private readonly HashSet<GameObject> knownTrayTiles =
        new HashSet<GameObject>();

    public void MoveTile(
        GameObject tile,
        Transform targetSlot)
    {
        if (tile == null ||
            targetSlot == null)
        {
            return;
        }

        RectTransform rect =
            tile.transform as RectTransform;

        if (rect == null)
        {
            Debug.LogWarning(
                $"MatchBoardMovement: {tile.name} has no RectTransform."
            );
            return;
        }

        // Remove destroyed pooled references.
        knownTrayTiles.RemoveWhere(
            trayTile => trayTile == null
        );

        bool isFirstTimeArrival =
            !knownTrayTiles.Contains(tile);

        if (isFirstTimeArrival)
        {
            knownTrayTiles.Add(tile);
        }

        // Kill previous movement/scale tweens
        // targeting this RectTransform.
        rect.DOKill();

        // Capture the target for this specific movement.
        Transform capturedTargetSlot = targetSlot;

        rect.DOMove(
                capturedTargetSlot.position,
                0.15f
            )
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                if (tile == null ||
                    rect == null ||
                    capturedTargetSlot == null)
                {
                    return;
                }

                // The tile may have been removed from the tray
                // while its movement animation was running.
                if (MatchBoard.instance == null ||
                    !MatchBoard.instance
                        .GetPlacedTiles()
                        .Contains(tile))
                {
                    return;
                }

                rect.position =
                    capturedTargetSlot.position;

                tile.transform.SetParent(
                    capturedTargetSlot,
                    true
                );

                tile.transform.SetAsLastSibling();

                rect.anchoredPosition =
                    Vector2.zero;

                if (isFirstTimeArrival)
                {
                    string fluffId =
                        tile.GetInstanceID() +
                        "_landingFluff";

                    DOTween.Kill(fluffId);

                    Sequence fluffSeq =
                        DOTween.Sequence()
                            .SetId(fluffId);

                    fluffSeq.Append(
                        rect.DOScale(
                            new Vector3(
                                0.95f,
                                0.75f,
                                1f
                            ),
                            0.08f
                        )
                        .SetEase(Ease.OutQuad)
                    );

                    fluffSeq.Append(
                        rect.DOScale(
                            Vector3.one * 0.9f,
                            0.15f
                        )
                        .SetEase(Ease.OutBack)
                    );
                }
                else
                {
                    rect.DOScale(
                        Vector3.one * 0.9f,
                        0.1f
                    );
                }
            });
    }

    public void ResetMovementState()
    {
        knownTrayTiles.Clear();
    }

    public void ForgetTrayTile(GameObject tile)
    {
        if (tile == null)
            return;

        knownTrayTiles.Remove(tile);

        // Kill the special landing animation too.
        string fluffId =
            tile.GetInstanceID() +
            "_landingFluff";

        DOTween.Kill(fluffId);
    }
}