using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinRewardAnimation : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private Button claimButton;
    private Vector3 startPos;

    void Start()
    {
        startPos = rewardText.rectTransform.localPosition;
    }

    public void ShowReward(int amount)
    {
        StopAllCoroutines();
        rewardText.gameObject.SetActive(true);

        rewardText.text = "+" + amount;
        claimButton.interactable = false;

        StartCoroutine(AnimationReward());
    }

    IEnumerator AnimationReward()
    {
        float time = 0;
        float duration = 1f;

        Vector3 endPos = startPos + new Vector3(0, 100, 0);

        Color startColor = rewardText.color;
        Color endColor = new Color(
            startColor.r,
            startColor.g,
            startColor.b,
            0
        );
        rewardText.rectTransform.localPosition = startPos;

        rewardText.color = startColor;

        while (time < duration)
        {
            float t = time / duration;

            rewardText.rectTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
            rewardText.color = Color.Lerp(startColor, endColor, t);

            time += Time.deltaTime;

            yield return null;
        }

        rewardText.gameObject.SetActive(false);
    }
}