using UnityEngine;

public class MatchBoardBooster : MonoBehaviour
{
    public void UseUndo()
    {
        if (TutorialManager.instance != null) TutorialManager.instance.CloseSoftTutorial();
        int currentLevel = PlayerPrefs.GetInt("Level", 0) + 1;
        bool isUnlocked = currentLevel > 3 || (currentLevel == 3 && PlayerPrefs.GetInt("UndoAnimPlayed", 0) == 1);

        if (!isUnlocked)
        {
            BoosterManager.instance.ShowBoosterLockedMessage("Unlocks at Level 3!");
            return;
        }

        if (MatchBoard.instance.isInputLocked) return;

        if (BoosterManager.instance.undoCount <= 0)
        {
            UIManager.Instance.ShowPopup(ScreenType.BuyUndoScreen);
            return;
        }

        if (!BoosterSystem.instance.CanUndo())
        {
            BoosterManager.instance.ShowNothingToUndo();
            return;
        }

        bool success = BoosterSystem.instance.UndoMove();
        if (success) BoosterManager.instance.UseUndo();
    }

    public void ShuffleTiles()
    {
        if (TutorialManager.instance != null) TutorialManager.instance.CloseSoftTutorial();
        int currentLevel = PlayerPrefs.GetInt("Level", 0) + 1;
        bool isUnlocked = currentLevel > 5 || (currentLevel == 5 && PlayerPrefs.GetInt("ShuffleAnimPlayed", 0) == 1);

        if (!isUnlocked)
        {
            BoosterManager.instance.ShowBoosterLockedMessage("Unlocks at Level 5!");
            return;
        }

        if (MatchBoard.instance.isInputLocked) return;

        if (BoosterManager.instance.shuffleCount <= 0)
        {
            UIManager.Instance.ShowPopup(ScreenType.BuyShuffleScreen);
            return;
        }

        if (!BoosterSystem.instance.CanShuffle())
        {
            BoosterManager.instance.ShowNothingToShuffle();
            return;
        }

        BoosterManager.instance.UseShuffle();
        BoosterSystem.instance.ShuffleTiles();
    }

    public void UseMagic()
    {
        if (TutorialManager.instance != null) TutorialManager.instance.CloseSoftTutorial();




        int currentLevel = PlayerPrefs.GetInt("Level", 0) + 1;
        bool isUnlocked = currentLevel > 7 || (currentLevel == 7 && PlayerPrefs.GetInt("MagicAnimPlayed", 0) == 1);

        if (!isUnlocked)
        {
            BoosterManager.instance.ShowBoosterLockedMessage("Unlocks at Level 7!");
            return;
        }

        if (MatchBoard.instance.isInputLocked) return;

        if (BoosterManager.instance.magicCount <= 0)
        {
            UIManager.Instance.ShowPopup(ScreenType.BuyMagicScreen);
            return;
        }

        if (!BoosterSystem.instance.CanUseMagic())
        {
            BoosterManager.instance.ShowNothingToMagic();
            return;
        }

        BoosterManager.instance.UseMagic();
        BoosterSystem.instance.UseMagicBooster();
    }
}