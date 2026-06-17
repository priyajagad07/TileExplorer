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

            if (slider.value >= slider.maxValue)
            {
                FinishLoading();
            }
        }
    }

    void FinishLoading()
    {
        isFinished = true;

        Debug.Log("Sliding completed");

        bool firstLaunch =
            PlayerPrefs.GetInt(
                "FirstGameplayLaunch",
                0
            ) == 0;

        if (firstLaunch)
        {
            PlayerPrefs.SetInt(
                "FirstGameplayLaunch",
                1
            );

            PlayerPrefs.Save();

            UIManager.Instance.Show(
                ScreenType.GamePlay
            );
        }
        else
        {
            UIManager.Instance.Show(
                ScreenType.HomeScreen
            );
        }
    }
}