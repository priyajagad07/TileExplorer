using UnityEngine;
using DG.Tweening;

public class MatchBoardBooster : MonoBehaviour
{
    private static bool isBoosterOnCooldown = false;

    public void UseUndo()
    {
        if (isBoosterOnCooldown) return;

        if (TutorialManager.instance != null) TutorialManager.instance.CloseSoftTutorial();
        int currentLevel = SaveManager.instance.data.level + 1;
        bool isUnlocked = currentLevel > 3 || (currentLevel == 3 && SaveManager.instance.data.undoAnimPlayed == 1);

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
        if (success)
        {
            BoosterManager.instance.UseUndo();
            StartCooldown(0.4f);
        }
    }

    public void ShuffleTiles()
    {
        if (isBoosterOnCooldown) return;

        if (TutorialManager.instance != null) TutorialManager.instance.CloseSoftTutorial();
        int currentLevel = SaveManager.instance.data.level + 1;
        bool isUnlocked = currentLevel > 5 || (currentLevel == 5 && SaveManager.instance.data.shuffleAnimPlayed == 1);

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

        StartCooldown(1.2f);
    }

    public void UseMagic()
    {
        if (isBoosterOnCooldown) return;

        if (TutorialManager.instance != null) TutorialManager.instance.CloseSoftTutorial();

        int currentLevel = SaveManager.instance.data.level + 1;
        bool isUnlocked = currentLevel > 7 || (currentLevel == 7 && SaveManager.instance.data.magicAnimPlayed == 1);

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

        StartCooldown(0.5f);
    }

    private void StartCooldown(float lockDuration)
    {
        isBoosterOnCooldown = true;

        DOVirtual.DelayedCall(lockDuration, () =>
        {
            isBoosterOnCooldown = false;
        });
    }
}