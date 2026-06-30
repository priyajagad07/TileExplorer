using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoardSection
{
    [Header("Placement")]
    public string groupName = "Group";
    public Vector2 position = Vector2.zero;

    [Header("Stack")]
    public StackStyle stackStyle = StackStyle.Standard;
    public float stackOffsetX = 30f;
    public float stackOffsetY = 30f;

    [Header("Layers")]
    public List<ShapeData> layers = new List<ShapeData>();
}