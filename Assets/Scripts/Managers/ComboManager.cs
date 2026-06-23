using UnityEngine;
using TMPro;
using DG.Tweening;

public class ComboManager : MonoBehaviour
{
    public static ComboManager instance;

    [Header("UI References")]
    [SerializeField] private TMP_Text comboText;

    [Header("Combo Settings")]
    [SerializeField] private float comboTimeLimit = 2.5f;

    private int currentCombo = 0;
    private float comboTimer = 0f;

    private readonly string[] feedbackWords = {
        "Good!",
        "Great!",
        "Awesome!",
        "Fabulous!",
        "Unbelievable!",
        "Godlike!",
        "Wowwww!",
    };

    void Awake()
    {
        instance = this;

        if (comboText != null)
        {
            comboText.gameObject.SetActive(false);
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
        if (comboText == null) return;

        DOTween.Kill(comboText.transform);
        DOTween.Kill(comboText);

        comboText.gameObject.SetActive(true);

        int wordIndex = Mathf.Min(currentCombo - 2, feedbackWords.Length - 1);
        string currentWord = feedbackWords[wordIndex];

        comboText.text = $"{currentWord}\n<size=60%>{currentCombo}x Combo!</size>";

        comboText.transform.localScale = Vector3.one * 0.5f;
        comboText.color = new Color(comboText.color.r, comboText.color.g, comboText.color.b, 1f);

        Sequence seq = DOTween.Sequence();

        seq.Append(comboText.transform.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack));
        seq.Append(comboText.transform.DOScale(1f, 0.15f));
        seq.AppendInterval(0.5f);
        seq.Append(comboText.transform.DOMoveY(comboText.transform.position.y + 50f, 0.4f).SetRelative(true));
        seq.Join(comboText.DOFade(0f, 0.4f));

        seq.OnComplete(() =>
        {
            comboText.gameObject.SetActive(false);
            comboText.transform.localPosition = Vector3.zero;
        });
    }
}