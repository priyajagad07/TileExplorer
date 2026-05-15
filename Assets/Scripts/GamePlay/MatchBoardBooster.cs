using System;
using UnityEngine;

public class MatchBoardBooster : MonoBehaviour
{
    public void UseUndo()
    {
        if (!BoosterManager.instance.UseUndo())
        {
            Debug.Log("No Undo Left");
            UIManager.Instance.ShowPopup(ScreenType.BuyUndoScreen);
            return;
        }

        BoosterSystem.instance.UndoMove();
    }

    public void ShuffleTiles()
    {
        if (!BoosterManager.instance.UseShuffle())
        {
            Debug.Log("No Shuffle Left");
            UIManager.Instance.ShowPopup(ScreenType.BuyShuffleScreen);
            return;
        }

        BoosterSystem.instance.ShuffleTiles();
    }

    public void UseMagic()
    {
        if (!BoosterManager.instance.UseMagic())
        {
            Debug.Log("No Magic Left");
            UIManager.Instance.ShowPopup(ScreenType.BuyMagicScreen);
            return;
        }

        BoosterSystem.instance.UseMagicBooster();
    }
}