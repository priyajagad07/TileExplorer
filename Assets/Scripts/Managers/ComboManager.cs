using UnityEngine;
using TMPro;
using DG.Tweening;

public class ComboManager : MonoBehaviour
{
    public static ComboManager instance;

    [Header("UI References")]
    [SerializeField] private TMP_Text comboText; // The text element that will pop up

    [Header("Combo Settings")]
    [SerializeField] private float comboTimeLimit = 2.5f; // How many seconds the player has to make another match
    
    private int currentCombo = 0;
    private float comboTimer = 0f;

    // The words that will appear as the combo gets higher!
    private readonly string[] feedbackWords = { 
        "Good!", 
        "Great!", 
        "Awesome!", 
        "Fabulous!", 
        "Unbelievable!", 
        "Godlike!" 
    };

    void Awake()
    {
        instance = this;
        
        if (comboText != null)
        {
            comboText.gameObject.SetActive(false); // Hide it at the start
        }
    }

    void Update()
    {
        // Countdown the timer
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            
            if (comboTimer <= 0)
            {
                // Timer ran out! Reset the combo
                currentCombo = 0;
            }
        }
    }

    // Call this exactly when a match of 3 is made!
    public void RegisterMatch()
    {
        // 1. Reset the timer
        comboTimer = comboTimeLimit;
        
        // 2. Increase the combo
        currentCombo++;

        // 3. Only show feedback if it's an actual combo (2 or more in a row)
        if (currentCombo > 1)
        {
            ShowComboFeedback();
        }
    }

    private void ShowComboFeedback()
    {
        if (comboText == null) return;

        // Stop any currently playing animations on the text so they don't glitch
        DOTween.Kill(comboText.transform);
        DOTween.Kill(comboText);

        comboText.gameObject.SetActive(true);

        // Pick the right word based on how high the combo is
        int wordIndex = Mathf.Min(currentCombo - 2, feedbackWords.Length - 1);
        string currentWord = feedbackWords[wordIndex];

        // Format the text (e.g., "Fabulous! \n 3x Combo")
        comboText.text = $"{currentWord}\n<size=60%>{currentCombo}x Combo!</size>";

        // Reset the visuals before animating
        comboText.transform.localScale = Vector3.one * 0.5f;
        comboText.color = new Color(comboText.color.r, comboText.color.g, comboText.color.b, 1f);

        // --- The DOTween Animation Sequence ---
        Sequence seq = DOTween.Sequence();

        // Pop up and overshoot (Bounce)
        seq.Append(comboText.transform.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack));
        
        // Settle to normal size
        seq.Append(comboText.transform.DOScale(1f, 0.15f));
        
        // Float there for half a second
        seq.AppendInterval(0.5f);
        
        // Float up slightly while fading out
        seq.Append(comboText.transform.DOMoveY(comboText.transform.position.y + 50f, 0.4f).SetRelative(true));
        seq.Join(comboText.DOFade(0f, 0.4f));

        // Hide the game object when finished
        seq.OnComplete(() => 
        {
            comboText.gameObject.SetActive(false);
            // Reset position for next time
            comboText.transform.localPosition = Vector3.zero; 
        });
    }
}