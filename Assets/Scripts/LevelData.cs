using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public GameObject[] tilePrefabs;
    public int totalTiles;
    public int gridRows;
    public int gridCols;
    public float spacing;

}