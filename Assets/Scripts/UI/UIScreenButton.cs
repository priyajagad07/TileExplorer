using UnityEngine;

public class UIScreenButton : MonoBehaviour
{
    [SerializeField] private ScreenType screenType;
    [SerializeField] private bool isPopup;

    public void OpenScreen()
    {
        if (isPopup)
        {
            UIManager.Instance.ShowPopup(screenType);
        }
        else
        {
            UIManager.Instance.Show(screenType);
        }
    }

    public void ClosePopup()
    {
        UIManager.Instance.HidePopup(screenType);
    }
}