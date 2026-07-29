using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class ComboManager : MonoBehaviour
{
    public static ComboManager instance;

    [Header("UI References")]
    [SerializeField] private GameObject comboUIContainer;
    [SerializeField] private Image currentImage;
    [SerializeField] private Image nextImage;
    [SerializeField] private CanvasGroup comboCanvasGroup;

    [Header("Sprites")]
    [SerializeField] private Sprite[] comboSprites;

    [Header("Settings")]
    [SerializeField] private float comboTimeLimit = 2.5f;
    [SerializeField] private float slideDistance = 120f;

    private int currentCombo;
    private float comboTimer;

    private Vector3 startPosition;
    private Queue<int> comboQueue = new Queue<int>();
    private bool isAnimating = false;

    private void Awake()
    {
        // Standard singleton setup
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        startPosition = comboUIContainer.transform.localPosition;
        comboUIContainer.SetActive(false);

        currentImage.color = Color.white;
        nextImage.color = new Color(1, 1, 1, 0);

        currentImage.rectTransform.anchoredPosition = Vector2.zero;
        nextImage.rectTransform.anchoredPosition = new Vector2(0, -slideDistance);
    }

    private void Update()
    {
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;

            if (comboTimer <= 0)
                currentCombo = 0;
        }
    }

    public void RegisterMatch()
    {
        comboTimer = comboTimeLimit;
        currentCombo++;

        if (currentCombo > 1)
            ShowComboFeedback();
    }

    private void ShowComboFeedback()
    {
        if (comboSprites == null || comboSprites.Length == 0)
            return;

        int index = Mathf.Min(currentCombo - 2, comboSprites.Length - 1);
        comboQueue.Enqueue(index);

        if (!isAnimating)
        {
            PlayNextCombo();
        }
    }

    private void PlayNextCombo()
    {
        if (comboQueue.Count == 0)
            return;

        isAnimating = true;
        int index = comboQueue.Dequeue();

        comboUIContainer.SetActive(true);

        // FIRST COMBO IN THE CHAIN (Pop-in animation)
        if (currentImage.sprite == null)
        {
            currentImage.sprite = comboSprites[index];
            comboCanvasGroup.alpha = 1;

            comboUIContainer.transform.localScale = Vector3.zero;
            comboUIContainer.transform.localPosition = startPosition;

            Sequence first = DOTween.Sequence();
            first.Append(comboUIContainer.transform.DOScale(1.25f, 0.18f).SetEase(Ease.OutBack));
            first.Append(comboUIContainer.transform.DOScale(1f, 0.12f));
            first.AppendInterval(0.6f); // Wait time to let player read the combo

            // Decide whether to slide next combo or exit
            first.AppendCallback(() =>
            {
                if (comboQueue.Count > 0)
                    PlayNextCombo();
                else
                    PlayExitAnimation();
            });
            return;
        }

        // CONTINUING COMBO (Slide animation)
        ChangeSprite(comboSprites[index]);
    }

    private void ChangeSprite(Sprite newSprite)
    {
        // 1. Prepare next image starting position (BELOW the center)
        nextImage.sprite = newSprite;
        nextImage.rectTransform.anchoredPosition = new Vector2(0, -slideDistance);
        nextImage.color = new Color(1, 1, 1, 0); // Start invisible

        Sequence seq = DOTween.Sequence();

        // 2. Current image goes UP (from 0 to +slideDistance)
        seq.Append(
            currentImage.rectTransform
                .DOAnchorPosY(slideDistance, 0.25f)
                .SetEase(Ease.OutBack) // Using OutBack on both keeps them perfectly in sync
        );
        seq.Join(
            currentImage.DOFade(0f, 0.25f)
        );

        // 3. Next image comes FROM BELOW (from -slideDistance to 0)
        seq.Join(
            nextImage.rectTransform
                .DOAnchorPosY(0f, 0.25f)
                .SetEase(Ease.OutBack)
        );
        seq.Join(
            nextImage.DOFade(1f, 0.25f)
        );

        // 4. Reset positions seamlessly after the animation finishes
        seq.AppendCallback(() =>
        {
            currentImage.sprite = nextImage.sprite;

            // Snap current image back to center so it's ready for the next combo
            currentImage.rectTransform.anchoredPosition = Vector2.zero;
            currentImage.color = Color.white;

            // Snap next image back to the bottom
            nextImage.rectTransform.anchoredPosition = new Vector2(0, -slideDistance);
            nextImage.color = new Color(1, 1, 1, 0);
        });

        // 5. Hold time to let the player read it
        seq.AppendInterval(0.6f);

        // 6. Decide whether to chain next combo or exit
        seq.AppendCallback(() =>
        {
            if (comboQueue.Count > 0)
                PlayNextCombo();
            else
                PlayExitAnimation();
        });
    }

    // Extracted the exit fade to prevent glitchy screen-blinking
    private void PlayExitAnimation()
    {
        Sequence exitSeq = DOTween.Sequence();

        exitSeq.Append(comboUIContainer.transform.DOLocalMoveY(startPosition.y + 60f, 0.35f).SetEase(Ease.OutQuad));
        exitSeq.Join(comboCanvasGroup.DOFade(0f, 0.35f));

        exitSeq.OnComplete(() =>
        {
            comboUIContainer.SetActive(false);
            comboUIContainer.transform.localPosition = startPosition;
            comboUIContainer.transform.localScale = Vector3.one;

            // [FIXED] Clear the sprite so a brand new combo chain pops in properly!
            currentImage.sprite = null;
            isAnimating = false;

            // Failsafe: In case a match was registered in the exact millisecond this was fading out
            if (comboQueue.Count > 0)
                PlayNextCombo();
        });
    }

    private void OnDestroy()
    {
        // Safe cleanup for DOTween
        DOTween.Kill(comboUIContainer.transform);
        DOTween.Kill(currentImage.rectTransform);
        DOTween.Kill(nextImage.rectTransform);
        DOTween.Kill(comboCanvasGroup);
    }
}