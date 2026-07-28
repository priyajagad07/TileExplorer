using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text amountText;

    public void Setup(RewardData reward)
    {
        icon.sprite = reward.rewardIcon;

        if (reward.rewardType == RewardType.Coins)
            amountText.text = reward.amount + " Coins";
        else
            amountText.text = "x" + reward.amount;
    }
}