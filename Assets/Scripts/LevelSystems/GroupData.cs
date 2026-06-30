using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GroupData
{
    public Vector2 position;

    public StackStyle stackStyle;

    public float stackOffsetX;
    public float stackOffsetY;

    public List<string[]> layerLayouts = new();
}