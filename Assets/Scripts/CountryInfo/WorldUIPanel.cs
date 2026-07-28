using UnityEngine;

public class WorldUIPanel : MonoBehaviour
{
    [Header("Data Reference")]
    [Tooltip("Drag the ScriptableObject for this world here.")]
    public WorldData worldData;

    [Header("UI Elements")]
    [Tooltip("Drag the DestinationCard child objects here IN ORDER (Card 1, Card 2, etc.)")]
    public DestinationCard[] destinationCards;
}