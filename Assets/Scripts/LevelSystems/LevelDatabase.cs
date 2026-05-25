using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "LevelDatabase",
    menuName = "Tile Explorer/Level Database"
)]
public class LevelDatabase : ScriptableObject
{
    public List<LevelData> levels =
        new List<LevelData>();
}