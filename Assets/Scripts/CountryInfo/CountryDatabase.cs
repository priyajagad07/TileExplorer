using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CountryDatabase",
    menuName = "Tile Explorer/Country Database"
)]
public class CountryDatabase : ScriptableObject
{
    public List<CountryData> countries;
}