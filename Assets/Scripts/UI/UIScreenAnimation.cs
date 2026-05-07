using System.Collections;
using UnityEngine;

public class UIScreenAnimation : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public AnimationType animationType;
    public Transform target;
    public float duration = 0.4f;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        if(target == null)
            target = transform;
    }

    public void Show()
    {
        Debug.Log("Animation Called");
        StopAllCoroutines();
        StartCoroutine(AnimationShow());
    }

    IEnumerator AnimationShow()
    {
        float time = 0;
        Vector3 startScale = Vector3.one;
        Vector3 startPos = target.localPosition;
        Vector3 endPos = Vector3.zero;

        canvasGroup.alpha = 0;

        switch (animationType)
        {
            case AnimationType.Scale:
                startScale = Vector3.one * 0.8f;
                target.localScale = startScale;
                break;
            
            case AnimationType.SlideDown:
                startPos = new Vector3(0, 800, 0);
                target.localPosition = startPos;
                break;

            case AnimationType.Zoom:
                startScale = Vector3.zero;
                target.localScale = startScale;
                break;
        }

        while(time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = time/duration;

            canvasGroup.alpha = t;

            switch (animationType)
            {
                case AnimationType.Scale:
                    target.localScale = Vector3.Lerp(startScale, Vector3.one, t);
                    break;
                
                case AnimationType.SlideDown:
                    target.localPosition = Vector3.Lerp(startPos, endPos, t);
                    break;

                case AnimationType.Zoom:
                    target.localScale = Vector3.Lerp(startScale, Vector3.one, t);
                    break;
            }
            yield return null;
        }
        canvasGroup.alpha= 1;
    }
}