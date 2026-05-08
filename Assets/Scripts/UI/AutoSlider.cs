using UnityEngine;
using UnityEngine.UI;

public class AutoSlider : MonoBehaviour
{
    public Slider slider;
    public float fillSpeed = 0.5f;
    private bool isFinished = false;

    void Update()
    {
        if (!isFinished)
        {
            slider.value += fillSpeed * Time.deltaTime;

            if(slider.value >= slider.maxValue)
            {
                FinishLoading();
            }
        }
    }

    void FinishLoading()
    {
        isFinished = true;
        Debug.Log("Sliding completed");
        UIManager.Instance.Show(ScreenType.HomeScreen);
    }
}