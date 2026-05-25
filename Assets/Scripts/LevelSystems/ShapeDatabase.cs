using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ShapeDatabase",
    menuName = "Tile Explorer/Shape Database"
)]
public class ShapeDatabase : ScriptableObject
{
    public List<ShapeData> shapes;
}