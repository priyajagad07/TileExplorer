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

        MoveToBoard();
    }

    public bool IsBlocked()
    {
        Transform parent = transform.parent;

        RectTransform myRect = GetComponent<RectTransform>();

        foreach (Transform child in parent)
        {
            if (child == transform)
                continue;

            Tile other = child.GetComponent<Tile>();

            if (other == null)
                continue;

            if(other.layer <= this.layer) //only checks tiles above this layer
                continue;

            RectTransform otherRect = other.GetComponent<RectTransform>();

            float distance = Vector2.Distance(
                myRect.anchoredPosition,
                otherRect.anchoredPosition
            );

            if(distance < 120f)
            {
                return true;
            }
        }
        return false;
    }

    public void SetMoved(bool value)
    {
        isMoved = value;
    }

    public void MoveToBoard()
    {
        if(isMoved)
            return;
        
        bool added = GameManager.instance.matchBoard.AddTile(gameObject);

        if (added)
        {
            isMoved = true;
            SoundManager.instance.PlaySound(SoundName.TileClick);
        }
    }

    public bool IsMoved()
    {
        return isMoved;
    }
}