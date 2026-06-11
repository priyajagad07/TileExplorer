using System.Collections.Generic;
using UnityEngine;

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

    [Header("Board")]

    public List<ShapeData> layers =
        new List<ShapeData>();
}