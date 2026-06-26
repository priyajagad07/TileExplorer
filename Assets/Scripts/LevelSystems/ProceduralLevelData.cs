using System;
using System.Collections.Generic;

[Serializable]
public class ProceduralLevelData
{
    public int rows;
    public int cols;
    public int layers;
    public float spacing;

    public string[] layout;
    public List<string[]> layerLayouts = new List<string[]>();

    public StackStyle stackStyle = StackStyle.Standard;
    public float stackOffsetX = 30f;
    public float stackOffsetY = 30f;
}