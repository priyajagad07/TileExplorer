using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class AutoShuffleManager : MonoBehaviour
{
    public static AutoShuffleManager instance;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text autoShuffleText; // The "No Matches! Shuffling..." popup text
    
    [Header("Settings")]
    [SerializeField] private int maxTraySlots = 7; // Change this if your tray holds more/less than 7

    void Awake()
    {
        instance = this;
        if (autoShuffleText != null)
        {
            autoShuffleText.gameObject.SetActive(false);
        }
    }

    // Call this method every time a tile is moved to the tray!
 public void CheckForDeadlock()
    {
        if (MatchBoard.instance == null || BoardSpawner.instance == null) return;

        List<GameObject> placedTiles = MatchBoard.instance.GetPlacedTiles();

        // 1. Only check for a deadlock if they are exactly 1 slot away from Game Over
        if (placedTiles.Count == maxTraySlots - 1)
        {
            // 2. Make a list of the tile IDs currently sitting in the tray
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

            // 3. Scan the board to see if ANY unblocked tile matches the tray
            foreach (Transform child in tileParent)
            {
                Tile tile = child.GetComponent<Tile>();

                if (tile == null || tile.IsMoved()) continue;

                // ---> THE FIX: Ask the tile directly if it is physically blocked! <---
                bool isUnblocked = !tile.IsBlocked(); 

                if (isUnblocked)
                {
                    // If an unblocked tile matches something in the tray, they have a move!
                    if (trayIds.Contains(tile.tileId))
                    {
                        hasValidMove = true;
                        break; 
                    }
                }
            }

            // 4. If there is no valid move, the player is doomed. Save them!
            if (!hasValidMove)
            {
                TriggerAutoShuffle();
            }
        }
    }
    
    private void TriggerAutoShuffle()
    {
        Debug.Log("Deadlock Detected! Saving player with Auto-Shuffle...");

        // Lock input instantly so the player doesn't click and lose while the text is popping up
        MatchBoard.instance.isInputLocked = true;

        if (autoShuffleText != null)
        {
            autoShuffleText.gameObject.SetActive(true);
            autoShuffleText.transform.localScale = Vector3.zero;
            autoShuffleText.color = new Color(autoShuffleText.color.r, autoShuffleText.color.g, autoShuffleText.color.b, 1f);

            // Pop the text onto the screen
            Sequence seq = DOTween.Sequence();
            seq.Append(autoShuffleText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
            seq.AppendInterval(1f); // Let them read it for 1 second
            seq.Append(autoShuffleText.DOFade(0f, 0.3f));
            seq.OnComplete(() => autoShuffleText.gameObject.SetActive(false));
        }

        // Wait half a second for the text animation, then trigger the global shuffle!
        DOVirtual.DelayedCall(0.5f, () =>
        {
            if (BoosterSystem.instance != null)
            {
                BoosterSystem.instance.ShuffleTiles();
            }
        });
    }
}