using UnityEngine;

[System.Serializable]
public class UndoData
{
    public GameObject tile;
    public Transform originalParent;
    public Vector2 originalPosition;
    public int siblingIndex;

    public Vector3 originalScale;
    public Vector2 originalSizeDelta;
    public Vector2 originalImageSizeDelta;

    public UndoData(
        GameObject tile,
        Transform parent,
        Vector2 position,
        int siblingIndex,
        Vector3 scale,
        Vector2 sizeDelta,
        Vector2 imageSizeDelta)
    {
        this.tile = tile;
        this.originalParent = parent;
        this.originalPosition = position;
        this.siblingIndex = siblingIndex;

        this.originalScale = scale;
        this.originalSizeDelta = sizeDelta;
        this.originalImageSizeDelta = imageSizeDelta;
    }
}