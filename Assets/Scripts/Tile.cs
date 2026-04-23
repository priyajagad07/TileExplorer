using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    private bool isMoved = false;
    public int tileId;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isMoved)
            return;

        if(IsBlocked())
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
        int myIndex = transform.GetSiblingIndex();

        for(int i = myIndex + 1; i<parent.childCount; i++)
        {
            Transform other = parent.GetChild(i);

            if(other.GetComponent<Tile>() != null)
            {
                float distance = Vector2.Distance(
                    transform.GetComponent<RectTransform>().anchoredPosition,
                    other.GetComponent<RectTransform>().anchoredPosition
                );

                if(distance < 100f)
                {
                    return true;
                }
            }
        }

        return false;
    }
}