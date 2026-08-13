using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class AutoShuffleManager : MonoBehaviour
{
    public static AutoShuffleManager instance;

    [Header("UI Elements")]
    [SerializeField] private CanvasGroup autoShufflePopupImage;
    [SerializeField] private TMP_Text autoShuffleText;

    [Header("Settings")]
    [SerializeField] private int maxTraySlots = 7;

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


        if (autoShufflePopupImage != null)
        {
            autoShufflePopupImage.gameObject.SetActive(false);
            autoShufflePopupImage.alpha = 0f;
        }
        else if (autoShuffleText != null)
        {
            autoShuffleText.gameObject.SetActive(false);
        }
    }

    public void CheckForDeadlock()
    {
        if (MatchBoard.instance == null || BoardSpawner.instance == null)
            return;

        // Jelly-only soft-lock can happen with any tray fill level.
        ResolveJellySoftLockIfNeeded();

        List<GameObject> placedTiles = MatchBoard.instance.GetPlacedTiles();

        if (placedTiles.Count == maxTraySlots - 1)
        {
            HashSet<int> trayIds = new HashSet<int>();
            foreach (GameObject t in placedTiles)
            {
                Tile tileScript = t.GetComponent<Tile>();
                if (tileScript != null)
                {
                    trayIds.Add(tileScript.tileId);
                }
            }

            Transform tileParent = BoardSpawner.instance.GetTileParent();
            bool hasValidMove = false;

            foreach (Transform child in tileParent)
            {
                if (!child.gameObject.activeSelf)
                    continue;

                Tile tile = child.GetComponent<Tile>();

                if (tile == null || tile.IsMoved())
                    continue;

                // Jelly cannot enter the tray, so it is never a valid move.
                if (tile.isJellyLocked)
                    continue;

                if (!tile.IsBlocked() &&
                    trayIds.Contains(tile.tileId))
                {
                    hasValidMove = true;
                    break;
                }
            }

            if (!hasValidMove)
            {
                TriggerAutoShuffle();
            }
        }
    }

    /// <summary>
    /// If every reachable board tile is jelly-locked, unlock jelly so
    /// the player cannot get permanently stuck.
    /// </summary>
    public void ResolveJellySoftLockIfNeeded()
    {
        if (BoardSpawner.instance == null)
            return;

        Transform tileParent = BoardSpawner.instance.GetTileParent();
        if (tileParent == null)
            return;

        if (HasPlayableNonJellyTile(tileParent))
            return;

        if (!HasRemainingJelly(tileParent))
            return;

        Debug.Log(
            "Jelly soft-lock detected. Unlocking reachable jelly tiles."
        );

        bool unlockedAny;
        int safety = 0;

        do
        {
            unlockedAny = false;
            safety++;

            Tile[] tiles =
                tileParent.GetComponentsInChildren<Tile>(false);

            for (int i = 0; i < tiles.Length; i++)
            {
                Tile tile = tiles[i];
                if (tile == null || tile.IsMoved())
                    continue;

                if (!tile.isJellyLocked)
                    continue;

                if (tile.IsBlocked())
                    continue;

                tile.ForceUnlockJelly();
                unlockedAny = true;
            }

            Tile.RefreshAllTileVisuals(tileParent);

            if (HasPlayableNonJellyTile(tileParent))
                break;

        } while (unlockedAny &&
                 HasRemainingJelly(tileParent) &&
                 safety < 32);

        // Absolute fallback: still soft-locked somehow.
        if (!HasPlayableNonJellyTile(tileParent) &&
            HasRemainingJelly(tileParent))
        {
            Tile[] tiles =
                tileParent.GetComponentsInChildren<Tile>(false);

            for (int i = 0; i < tiles.Length; i++)
            {
                Tile tile = tiles[i];
                if (tile != null &&
                    !tile.IsMoved() &&
                    tile.isJellyLocked)
                {
                    tile.ForceUnlockJelly();
                }
            }

            Tile.RefreshAllTileVisuals(tileParent);
        }
    }

    private static bool HasPlayableNonJellyTile(Transform tileParent)
    {
        foreach (Transform child in tileParent)
        {
            if (!child.gameObject.activeSelf)
                continue;

            Tile tile = child.GetComponent<Tile>();
            if (tile == null || tile.IsMoved())
                continue;

            if (tile.isJellyLocked)
                continue;

            if (!tile.IsBlocked())
                return true;
        }

        return false;
    }

    private static bool HasRemainingJelly(Transform tileParent)
    {
        foreach (Transform child in tileParent)
        {
            if (!child.gameObject.activeSelf)
                continue;

            Tile tile = child.GetComponent<Tile>();
            if (tile == null || tile.IsMoved())
                continue;

            if (tile.isJellyLocked)
                return true;
        }

        return false;
    }

    private void TriggerAutoShuffle()
    {
        Debug.Log("Deadlock Detected! Saving player with Auto-Shuffle...");

        MatchBoard.instance.isInputLocked = true;

        if (autoShufflePopupImage != null)
        {
            autoShufflePopupImage.gameObject.SetActive(true);
            autoShufflePopupImage.alpha = 1f;

            autoShufflePopupImage.transform.DOKill();
            autoShufflePopupImage.transform.localScale = Vector3.one;
            autoShufflePopupImage.transform.DOPunchScale(Vector3.one * 0.15f, 0.35f, 5, 0.5f);

            DOVirtual.DelayedCall(1.5f, () =>
            {
                autoShufflePopupImage.DOFade(0f, 0.4f).OnComplete(() => autoShufflePopupImage.gameObject.SetActive(false));
            });
        }
        else if (autoShuffleText != null)
        {
            autoShuffleText.gameObject.SetActive(true);
            autoShuffleText.transform.localScale = Vector3.zero;
            autoShuffleText.color = new Color(autoShuffleText.color.r, autoShuffleText.color.g, autoShuffleText.color.b, 1f);

            Sequence seq = DOTween.Sequence();
            seq.Append(autoShuffleText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
            seq.AppendInterval(1f);
            seq.Append(autoShuffleText.DOFade(0f, 0.3f));
            seq.OnComplete(() => autoShuffleText.gameObject.SetActive(false));
        }

        DOVirtual.DelayedCall(0.5f, () =>
        {
            if (BoosterSystem.instance != null)
            {
                BoosterSystem.instance.ShuffleTiles();
            }
        });
    }
}
