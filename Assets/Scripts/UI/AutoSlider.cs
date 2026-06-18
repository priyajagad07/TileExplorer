using UnityEngine;
using UnityEngine.UI;

public class AutoSlider : MonoBehaviour
{
    public static bool isSliding = true;
    public Slider slider;
    public float fillSpeed = 0.5f;
    private bool isFinished = false;

    void Awake()
    {
        isSliding = true;
    }

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
        isSliding = false; 
        Debug.Log("Sliding completed");

        bool firstLaunch = SaveManager.instance.data.firstGameplayLaunch == 0;

        if (firstLaunch)
        {
            SaveManager.instance.data.firstGameplayLaunch = 1;
            SaveManager.instance.SaveData();
            UIManager.Instance.Show(ScreenType.GamePlay);
        }
        else
        {
            UIManager.Instance.Show(ScreenType.HomeScreen);
        }
    }
}