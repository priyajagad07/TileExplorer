using UnityEngine;

[System.Serializable]
public class DestinationData
{
    public string destinationName;

    [TextArea(2,5)]
    public string description;

    public Sprite background;
}