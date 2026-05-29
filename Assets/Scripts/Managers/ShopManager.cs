using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public void BuySuperBundle()
    {
        CoinManager.instance.AddCoins(1200);

        BoosterManager.instance.AddBoosters(
            3, // Undo
            3, // Shuffle
            3  // Magic
        );

        Debug.Log("Super Bundle Purchased");
    }

    public void Buy1500Coins()
    {
        CoinManager.instance.AddCoins(1500);

        Debug.Log("1500 Coins Purchased");
    }
}