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
        instance = this;
        
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
        if (MatchBoard.instance == null || BoardSpawner.instance == null) return;

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
                Tile tile = child.GetComponent<Tile>();

                if (tile == null || tile.IsMoved()) continue;

                bool isUnblocked = !tile.IsBlocked(); 

                if (isUnblocked)
                {
                    if (trayIds.Contains(tile.tileId))
                    {
                        hasValidMove = true;
                        break; 
                    }
                }
            }

            if (!hasValidMove)
            {
                TriggerAutoShuffle();
            }
        }
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