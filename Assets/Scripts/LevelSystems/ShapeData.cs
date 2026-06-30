using UnityEngine;

[CreateAssetMenu(
    fileName = "ShapeData",
    menuName = "Tile Explorer/Shape Data"
)]
public class ShapeData : ScriptableObject
{
    public string shapeName;

    public string[] layout;
}