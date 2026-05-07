using UnityEngine;

[System.Serializable]
public class UndoData
{
    public GameObject tile;
    public Transform originalParent;
    public Vector2 originalPosition;
    public int siblingIndex;

    public UndoData(
        GameObject tile,
        Transform parent,
        Vector2 position,
        int siblingIndex
    )
    {
        this.tile = tile;
        this.originalParent = parent;
        this.originalPosition = position;
        this.siblingIndex = siblingIndex;
    }
}