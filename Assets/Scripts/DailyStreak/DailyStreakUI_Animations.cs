using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public partial class DailyStreakUI
{
    public void PlayRewardSequence()
    {
        Debug.Log("PlayRewardSequence");
        DOTween.Kill("RewardSeq");

        int streak = DailyStreakManager.instance.GetStreak();

        ShowPreviousProgress(streak);

        Sequence seq = DOTween.Sequence().SetId("RewardSeq");

        seq.AppendInterval(0.35f);

        seq.AppendCallback(() =>
        {
            ClaimDay(streak);
        });

        seq.AppendInterval(0.35f);

        seq.AppendCallback(() =>
        {
            ShowRewardPopup(DailyRewardManager.instance.GetRewardForDay(streak));
        });
    }

    void ClaimDay(int streak)
    {
        if (streak <= 0) return;

        DaySlotUI slot = daySlots[streak - 1];

        RectTransform chest = slot.chest.rectTransform;
        RectTransform bg = slot.background.rectTransform;
        Vector2 startPos = chest.anchoredPosition;

        chest.DOKill(true);
        bg.DOKill(true);

        chest.anchoredPosition = startPos;
        chest.sizeDelta = new Vector2(normalChestSize, normalChestSize);

        Sequence seq = DOTween.Sequence();

        // Squash
        seq.Append(
            chest.DOSizeDelta(
                new Vector2(105, 105),
                0.08f
            ).SetEase(Ease.InQuad)
        );

        // Shake
        seq.Append(
            chest.DOShakeRotation(
                0.10f,
                new Vector3(0, 0, 8),
                12,
                90,
                false
            )
        );

        seq.Join(chest.DOAnchorPosY(startPos.y + 12f, 0.12f).SetLoops(2, LoopType.Yoyo));
        // Tiny pause before opening
        seq.AppendInterval(0.05f);

        // Activate today's tab
        seq.AppendCallback(() =>
        {
            slot.background.gameObject.SetActive(true);
            bg.localScale = Vector3.zero;
            slot.background.sprite = activeTabSprite;
            slot.dayText.DOColor(activeTextColor, 0.15f);
            slot.chest.sprite = claimedChestSprite;

            SoundManager.instance.PlaySound(SoundName.RewardReveal);
        });

        // Chest Pop
        seq.Append(
            chest.DOSizeDelta(
                new Vector2(popChestSize, popChestSize),
                0.18f
            ).SetEase(Ease.OutBack)
        );

        seq.Append(
            chest.DOSizeDelta(
                new Vector2(claimedChestSize, claimedChestSize),
                0.12f
            )
        );

        // Chest rotation
        seq.Join(
            chest.DOPunchRotation(
                new Vector3(0, 0, 12),
                0.25f,
                8,
                1
            )
        );

        // Tab pop
        seq.Join(
            bg.DOScale(1.12f, 0.15f)
                .SetEase(Ease.OutBack)
        );

        seq.Append(
            bg.DOScale(1f, 0.12f)
                .SetEase(Ease.OutQuad)
        );

        // Text punch
        seq.Join(
            slot.dayText.transform.DOPunchScale(
                Vector3.one * 0.15f,
                0.25f
            )
        );
    }

    void ShowPreviousProgress(int streak)
    {
        for (int i = 0; i < daySlots.Length; i++)
        {
            bool claimed = i < streak - 1;

            if (claimed)
            {
                daySlots[i].background.gameObject.SetActive(true);
                daySlots[i].background.sprite = activeTabSprite;
                daySlots[i].dayText.color = activeTextColor;
            }
            else
            {
                daySlots[i].background.gameObject.SetActive(false);
                daySlots[i].dayText.color = inactiveTextColor;
            }

            daySlots[i].chest.sprite =
                claimed ? claimedChestSprite : lockedChestSprite;

            RectTransform rect = daySlots[i].chest.rectTransform;

            rect.sizeDelta = new Vector2(
                claimed ? claimedChestSize : normalChestSize,
                claimed ? claimedChestSize : normalChestSize
            );
        }
    }
}