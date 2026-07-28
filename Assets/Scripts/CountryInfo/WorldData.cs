using UnityEngine;

[CreateAssetMenu(
    fileName = "WorldData",
    menuName = "Tile Explorer/World Data"
)]
public class WorldData : ScriptableObject
{
    [Header("World Info")]
    public string worldName;

    public int startLevel;
    public int endLevel;

    [Header("Backgrounds")]
    public Sprite[] backgrounds;

    [Header("Map Preview Cards")]
    public Sprite[] previewCards;

    [Header("Destinations")]
    public DestinationData[] destinations;
}