using UnityEngine;
using DG.Tweening;

public class MatchBoardBooster : MonoBehaviour
{
    private static bool isBoosterOnCooldown = false;

    public void UseUndo()
    {
        if (isBoosterOnCooldown)
            return;

        bool undoTutorialRunning =
            UndoTutorialManager.instance != null &&
            UndoTutorialManager.instance
                .IsRunning;

        bool freeTutorialUndo =
            UndoTutorialManager.instance != null &&
            UndoTutorialManager.instance
                .IsWaitingForUndoTap;

        /*
         * Soft tutorial:
         *
         * If player presses Undo before doing the
         * requested tile step, the free opportunity
         * is lost and normal Undo logic continues.
         */
        if (undoTutorialRunning &&
            !freeTutorialUndo)
        {
            UndoTutorialManager.instance
                .CancelTutorial();
        }

        int currentLevel =
            SaveManager.instance.data.level + 1;

        bool isUnlocked =
            currentLevel > 3 ||
            (
                currentLevel == 3 &&
                SaveManager.instance.data
                    .undoAnimPlayed == 1
            );

        if (!isUnlocked)
        {
            BoosterManager.instance
                .ShowBoosterLockedMessage(
                    "Unlocks at Level 3!"
                );

            return;
        }

        if (MatchBoard.instance.isInputLocked)
            return;

        /*
         * Tutorial Undo is temporary and FREE.
         *
         * Normal Undo checks inventory.
         */
        if (!freeTutorialUndo &&
            BoosterManager.instance.undoCount <= 0)
        {
            UIManager.Instance.ShowPopup(
                ScreenType.BuyUndoScreen
            );

            StartCooldown(0.5f);
            return;
        }

        if (!BoosterSystem.instance.CanUndo())
        {
            if (BoosterSystem.instance.justShuffled)
            {
                BoosterManager.instance
                    .ShowCannotUndoShuffle();
            }
            else
            {
                BoosterManager.instance
                    .ShowNothingToUndo();
            }

            return;
        }

        bool success =
            BoosterSystem.instance
                .UndoMove();

        if (!success)
            return;

        /*
         * Do NOT consume an Undo
         * during the tutorial.
         */
        if (!freeTutorialUndo)
        {
            BoosterManager.instance
                .UseUndo();
        }

        if (freeTutorialUndo &&
            UndoTutorialManager.instance != null)
        {
            UndoTutorialManager.instance
                .CompleteFreeUndo();
        }

        MatchBoard.instance
            .isInputLocked = false;

        StartCooldown(0.4f);
    }
    public void ShuffleTiles()
    {
        if (isBoosterOnCooldown)
            return;

        if (UndoTutorialManager.instance != null &&
     UndoTutorialManager.instance.IsRunning)
        {
            UndoTutorialManager.instance
                .CancelTutorial();
        }

        bool freeTutorialUse =
            TutorialManager.instance != null &&
            TutorialManager.instance
                .IsBoosterTutorialRunning(
                    "Shuffle"
                );

        int currentLevel =
            SaveManager.instance.data.level + 1;

        bool isUnlocked =
            currentLevel > 5 ||
            (
                currentLevel == 5 &&
                SaveManager.instance.data
                    .shuffleAnimPlayed == 1
            );

        if (!isUnlocked)
        {
            BoosterManager.instance
                .ShowBoosterLockedMessage(
                    "Unlocks at Level 5!"
                );

            return;
        }

        if (MatchBoard.instance.isInputLocked)
            return;

        // Tutorial Shuffle is FREE.
        if (!freeTutorialUse &&
            BoosterManager.instance.shuffleCount <= 0)
        {
            UIManager.Instance.ShowPopup(
                ScreenType.BuyShuffleScreen
            );

            StartCooldown(0.5f);
            return;
        }

        if (!BoosterSystem.instance.CanShuffle())
        {
            BoosterManager.instance
                .ShowNothingToShuffle();

            return;
        }

        if (!freeTutorialUse)
        {
            BoosterManager.instance.UseShuffle();
        }

        BoosterSystem.instance.ShuffleTiles();

        if (freeTutorialUse)
        {
            TutorialManager.instance
                .CloseSoftTutorial();
        }

        StartCooldown(1.2f);
    }

    public void UseMagic()
    {
        if (isBoosterOnCooldown)
            return;

        if (UndoTutorialManager.instance != null && UndoTutorialManager.instance.IsRunning)
        {
            UndoTutorialManager.instance
                .CancelTutorial();
        }

        bool freeTutorialUse =
            TutorialManager.instance != null &&
            TutorialManager.instance
                .IsBoosterTutorialRunning(
                    "Magic"
                );

        int currentLevel =
            SaveManager.instance.data.level + 1;

        bool isUnlocked =
            currentLevel > 7 ||
            (
                currentLevel == 7 &&
                SaveManager.instance.data
                    .magicAnimPlayed == 1
            );

        if (!isUnlocked)
        {
            BoosterManager.instance
                .ShowBoosterLockedMessage(
                    "Unlocks at Level 7!"
                );

            return;
        }

        if (MatchBoard.instance.isInputLocked)
            return;

        // Tutorial Magic is FREE.
        if (!freeTutorialUse &&
            BoosterManager.instance.magicCount <= 0)
        {
            UIManager.Instance.ShowPopup(
                ScreenType.BuyMagicScreen
            );

            StartCooldown(0.5f);
            return;
        }

        if (!BoosterSystem.instance.CanUseMagic())
        {
            BoosterManager.instance
                .ShowNothingToMagic();

            return;
        }

        if (!freeTutorialUse)
        {
            BoosterManager.instance.UseMagic();
        }

        BoosterSystem.instance
            .UseMagicBooster();

        if (freeTutorialUse)
        {
            TutorialManager.instance
                .CloseSoftTutorial();
        }

        StartCooldown(1f);
    }

    private void StartCooldown(float lockDuration)
    {
        isBoosterOnCooldown = true;

        DOVirtual.DelayedCall(
            lockDuration,
            () =>
            {
                isBoosterOnCooldown = false;
            }
        )
        .SetUpdate(true);
    }

}