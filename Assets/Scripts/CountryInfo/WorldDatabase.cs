using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "WorldDatabase",
    menuName = "Tile Explorer/World Database"
)]
public class WorldDatabase : ScriptableObject
{
    public List<WorldData> worlds;
}