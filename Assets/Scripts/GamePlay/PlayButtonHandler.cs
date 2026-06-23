using UnityEngine;
using DG.Tweening;

public class PlayButtonHandler : MonoBehaviour
{
    private bool isHandlingClick = false;

    public void OnPlayClicked()
    {
        if (isHandlingClick) return;
        
        isHandlingClick = true;
        DOVirtual.DelayedCall(0.5f, () => isHandlingClick = false); 

        if (GameManager.instance != null && GameManager.instance.isGameInProgress)
        {
            UIManager.Instance.ShowPopup(ScreenType.ContinueGame);
        }
        else
        {
            UIManager.Instance.Show(ScreenType.GamePlay);
            
            if (BoardSpawner.instance != null && MatchBoard.instance.GetPlacedTiles().Count == 0)
            {
                int currentLevel = SaveManager.instance.data.level;
                LevelManager.instance.LoadLevel(currentLevel);
                
                DOVirtual.DelayedCall(0.1f, () =>
                {
                    if (BoardSpawner.instance != null)
                    {
                        BoardSpawner.instance.PlaySpawnAnimation();
                    }
                });
            }
        }
    }
}