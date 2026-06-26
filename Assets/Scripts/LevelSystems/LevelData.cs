using System.Collections.Generic;
using UnityEngine;

public enum StackStyle
{
    Standard,
    ZigZag,
    Cascade
}

[CreateAssetMenu(
    fileName = "LevelData",
    menuName = "Tile Explorer/Level Data"
)]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public int levelNumber;
    public int rewardCoins = 100;
    public int difficulty = 1;

    [Header("Stack Settings")]
    public StackStyle stackStyle = StackStyle.Standard;
    public float stackOffsetX = 30f;
    public float stackOffsetY = 30f;

    [Header("Board")]
    public List<ShapeData> layers = new List<ShapeData>();
}