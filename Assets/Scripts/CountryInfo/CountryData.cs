using UnityEngine;

[CreateAssetMenu(
    fileName = "CountryData",
    menuName = "Tile Explorer/Country Data"
)]
public class CountryData : ScriptableObject
{
    [Header("Country Info")]
    public string countryName;

    public int startLevel;
    public int endLevel;

    [Header("Backgrounds")]
    public Sprite[] backgrounds;

    [Header("Map Preview Cards")]
    public Sprite[] previewCards;

    [Header("Destinations")]
    public DestinationData[] destinations;
}