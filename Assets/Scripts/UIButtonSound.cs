using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    public SoundName sound = SoundName.ButtonClick;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }

    void PlaySound()
    {
        if(SoundManager.instance != null)
        {
            SoundManager.instance.PlaySound(sound);
        }
    }
}