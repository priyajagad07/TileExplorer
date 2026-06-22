using UnityEngine;
using UnityEngine.UI;

public class AutoSlider : MonoBehaviour
{
    public static bool isSliding = true;
    public Slider slider;

    [Header("Speed Settings")]
    public float verySlowSpeed = 0.1f;   // Speed for 0% to 20%
    public float normalSlowSpeed = 0.4f; // Speed for 20% to 80%
    public float littleFastSpeed = 1.2f; // Speed for 80% to 100%

    [Header("Phase Thresholds")]
    [Range(0f, 1f)] public float phase1End = 0.20f; // 20% mark
    [Range(0f, 1f)] public float phase2End = 0.80f; // 80% mark

    private bool isFinished = false;

    void Awake()
    {
        isSliding = true;
        
        if (slider != null)
        {
            slider.value = 0; 
        }
    }

    void Update()
    {
        if (!isFinished)
        {
            float fillPercentage = slider.value / slider.maxValue;
            float currentSpeed = 0f;

            if (fillPercentage < phase1End)
            {
                currentSpeed = verySlowSpeed;
            }
            else if (fillPercentage < phase2End)
            {
                currentSpeed = normalSlowSpeed;
            }
            else
            {
                currentSpeed = littleFastSpeed;
            }

            slider.value += currentSpeed * Time.deltaTime;

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