using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    private bool isMoved = false;
    public int tileId;
    public int row;
    public int col;
    public int layer;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isMoved)
            return;

        if (IsBlocked())
            return;

        bool added = GameManager.instance.matchBoard.AddTile(gameObject);

        if (added)
        {
            isMoved = true;
        }
    }

    bool IsBlocked()
    {
        Transform parent = transform.parent;

        foreach (Transform child in parent)
        {
            if (child == transform)
                continue;

            Tile other = child.GetComponent<Tile>();

            if (other == null)
                continue;

            if(other.layer > this.layer && other.row == this.row && other.col == this.col)
            {
                return true;
            }
        }
        return false;
    }
}