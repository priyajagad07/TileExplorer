using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public partial class DailyStreakUI
{
    public void PlayRewardSequence()
    {

        Debug.Log("PlayRewardSequence");
        int streak =
            DailyStreakManager.instance
                .GetStreak();

        ShowPreviousProgress(streak);

        keepGoingButton.localScale =
            Vector3.zero;

        PlayBirdAnimation();

        Sequence seq =
            DOTween.Sequence();

        seq.AppendInterval(0.5f);

        seq.AppendCallback(() =>
        {
            AnimateStreak(streak);
        });

        seq.AppendInterval(1f);

        seq.AppendCallback(() =>
        {
            ClaimDay(streak);
        });

        seq.AppendInterval(0.25f);

        seq.AppendCallback(() =>
        {
            ShowRewardPopup(
                DailyRewardManager.instance
                    .GetRewardTextForDay(
                        streak
                    )
            );
        });

        seq.AppendInterval(0.3f);

        seq.AppendCallback(() =>
        {
            DailyStreakManager.instance
                .GiveRewardForCurrentDay();
        });

        seq.AppendInterval(0.3f);

        seq.AppendCallback(() =>
        {
            ShowKeepGoing();
        });
    }

    void AnimateStreak(int targetValue)
    {
        if (streakText == null)
            return;

        int previous =
            Mathf.Max(
                0,
                targetValue - 1
            );

        SoundManager.instance.PlaySound(
            SoundName.CounterStart
        );

        DOTween.To(
            () => previous,
            x =>
            {
                streakText.text =
                    x.ToString();
            },
            targetValue,
            0.5f
        )
        .OnComplete(() =>
        {
            SoundManager.instance.PlaySound(
                SoundName.NumberPop
            );

            streakText.transform
                .DOPunchScale(
                    Vector3.one * 0.3f,
                    0.35f,
                    10,
                    1
                );
        });
    }

    void PlayBirdAnimation()
    {
        birdParent.localScale =
            Vector3.zero;

        SoundManager.instance.PlaySound(
            SoundName.BirdPop
        );

        birdParent
            .DOScale(
                1.15f,
                0.3f
            )
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                birdParent
                    .DOScale(
                        1f,
                        0.15f
                    );
            });
    }

    void ShowKeepGoing()
    {
         Debug.Log("ShowKeepGoing");
        SoundManager.instance.PlaySound(
            SoundName.ButtonPop
        );

        keepGoingButton
            .DOScale(
                1.1f,
                0.3f
            )
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                keepGoingButton
                    .DOScale(
                        1f,
                        0.1f
                    );
            });
    }

    void ClaimDay(int streak)
    {
        if (streak <= 0)
            return;

        Image icon =
            dayIcons[streak - 1];

        icon.color =
            completedColor;

        icon.transform.localScale =
            Vector3.one * 0.7f;

        Sequence seq =
            DOTween.Sequence();

        seq.Append(
            icon.transform
                .DOScale(
                    1.35f,
                    0.2f
                )
        );

        seq.Append(
            icon.transform
                .DOScale(
                    1f,
                    0.15f
                )
        );

        seq.Join(
            icon.transform
                .DOPunchRotation(
                    new Vector3(
                        0,
                        0,
                        15
                    ),
                    0.35f,
                    8,
                    1
                )
        );
    }

    void ShowPreviousProgress(
        int streak
    )
    {
        for (
            int i = 0;
            i < dayIcons.Length;
            i++
        )
        {
            dayIcons[i].color =
                i < streak - 1
                ? completedColor
                : defaultColor;
        }
    }
}