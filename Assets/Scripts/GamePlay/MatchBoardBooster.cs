using UnityEngine;

public class MatchBoardBooster : MonoBehaviour
{
    public void UseUndo()
    {
        if (BoosterManager.instance.undoCount <= 0)
        {
            Debug.Log("No Undo Left");
            UIManager.Instance.ShowPopup(ScreenType.BuyUndoScreen);
            return;
        }

        if (!BoosterSystem.instance.CanUndo())
        {
            BoosterManager.instance.ShowNothingToUndo();
            Debug.Log("Nothing To Undo");
            return;
        }

        BoosterManager.instance.UseUndo();

        bool success = BoosterSystem.instance.UndoMove();

        if (!success)
        {
            return;
        }
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