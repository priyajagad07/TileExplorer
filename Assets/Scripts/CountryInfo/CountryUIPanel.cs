using UnityEngine;

public class CountryUIPanel : MonoBehaviour
{
    [Header("Data Reference")]
    [Tooltip("Drag the ScriptableObject for this country here.")]
    public CountryData countryData;

    [Header("UI Elements")]
    [Tooltip("Drag the DestinationCard child objects here IN ORDER (Card 1, Card 2, etc.)")]
    public DestinationCard[] destinationCards;
}