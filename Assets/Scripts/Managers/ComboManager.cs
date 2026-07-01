using UnityEngine;
using TMPro;
using DG.Tweening;

public class ComboManager : MonoBehaviour
{
    public static ComboManager instance;

    [Header("UI References")]
    [SerializeField] private GameObject comboUIContainer; // Drag the 'ComboFeedback - Image' here!
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private CanvasGroup comboCanvasGroup; // Drag the 'ComboFeedback - Image' here too!

    [Header("Combo Settings")]
    [SerializeField] private float comboTimeLimit = 2.5f;

    private int currentCombo = 0;
    private float comboTimer = 0f;
    private Vector3 startPosition;

    // Added a bunch of new words for you!
    private readonly string[] feedbackWords = {
        "Nice!",
        "Good!",
        "Great!",
        "Super!",
        "Awesome!",
        "Amazing!",
        "Fabulous!",
        "Fantastic!",
        "Brilliant!",
        "Unbelievable!",
        "Godlike!",
        "Legendary!",
        "Woww!!"
    };

    void Awake()
    {
        instance = this;

        if (comboUIContainer != null)
        {
            // Remember exactly where you placed it in the editor
            startPosition = comboUIContainer.transform.localPosition;
            comboUIContainer.SetActive(false);
        }
    }

    void Update()
    {
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;

            if (comboTimer <= 0)
            {
                currentCombo = 0;
            }
        }
    }

    public void RegisterMatch()
    {
        comboTimer = comboTimeLimit;

        currentCombo++;
        if (currentCombo > 1)
        {
            ShowComboFeedback();
        }
    }

    private void ShowComboFeedback()
    {
        if (comboUIContainer == null || comboText == null || comboCanvasGroup == null) return;

        // Kill old animations so spam-matching doesn't break it
        DOTween.Kill(comboUIContainer.transform);
        DOTween.Kill(comboCanvasGroup);

        comboUIContainer.SetActive(true);

        // Pick a word based on the combo length
        int wordIndex = Mathf.Min(currentCombo - 2, feedbackWords.Length - 1);
        comboText.text = feedbackWords[wordIndex]; // No more numbers! Just the word.

        // Reset scale, position, and alpha before animating
        comboUIContainer.transform.localScale = Vector3.one * 0.4f; // Start slightly small
        comboUIContainer.transform.localPosition = startPosition;
        comboCanvasGroup.alpha = 1f;

        Sequence seq = DOTween.Sequence();

        // 1. Pop up to the EXACT original size (Vector3.one) that you set in the Editor
        seq.Append(comboUIContainer.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack));
        
        // 2. Hold on screen for a moment so the player can read it
        seq.AppendInterval(0.6f);
        
        // 3. Float up slightly and fade out both the image and text together
        seq.Append(comboUIContainer.transform.DOMoveY(comboUIContainer.transform.position.y + 45f, 0.4f));
        seq.Join(comboCanvasGroup.DOFade(0f, 0.4f));

        // 4. Hide it safely when done
        seq.OnComplete(() =>
        {
            comboUIContainer.SetActive(false);
            comboUIContainer.transform.localPosition = startPosition;
        });
    }
}