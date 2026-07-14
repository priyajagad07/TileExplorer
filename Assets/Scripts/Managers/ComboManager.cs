using UnityEngine;
using TMPro;
using DG.Tweening;

public class ComboManager : MonoBehaviour
{
    public static ComboManager instance;

    [Header("UI References")]
    [SerializeField] private GameObject comboUIContainer;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private CanvasGroup comboCanvasGroup;
    [Header("Combo Settings")]
    [SerializeField] private float comboTimeLimit = 2.5f;

    private int currentCombo = 0;
    private float comboTimer = 0f;
    private Vector3 startPosition;

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
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (comboUIContainer != null)
        {
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

        DOTween.Kill(comboUIContainer.transform);
        DOTween.Kill(comboCanvasGroup);

        comboUIContainer.SetActive(true);

        int wordIndex = Mathf.Min(currentCombo - 2, feedbackWords.Length - 1);
        comboText.text = feedbackWords[wordIndex];

        comboUIContainer.transform.localScale = Vector3.one * 0.4f;
        comboUIContainer.transform.localPosition = startPosition;
        comboCanvasGroup.alpha = 1f;

        Sequence seq = DOTween.Sequence();

        seq.Append(comboUIContainer.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack));

        seq.AppendInterval(0.6f);

        seq.Append(comboUIContainer.transform.DOMoveY(comboUIContainer.transform.position.y + 45f, 0.4f));
        seq.Join(comboCanvasGroup.DOFade(0f, 0.4f));

        seq.OnComplete(() =>
        {
            comboUIContainer.SetActive(false);
            comboUIContainer.transform.localPosition = startPosition;
        });
    }
}