using UnityEngine;

public class UIInputBlocker : MonoBehaviour
{
    public static UIInputBlocker instance;
    public GameObject blockerPanel;
    public bool IsBlocked => blockerPanel.activeSelf;

    void Awake()
    {
        instance = this;
        blockerPanel.SetActive(false);
    }

    public void Block()
    {
        blockerPanel.transform.SetAsLastSibling();
        blockerPanel.SetActive(true);
    }

    public void Unblock()
    {
        blockerPanel.SetActive(false);
    }
}