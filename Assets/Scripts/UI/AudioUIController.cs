using UnityEngine;
using UnityEngine.UI;

public class AudioUIController : MonoBehaviour
{
    public Image musicIcon;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    public Image sfxIcon;
    public Sprite sfxOnSprite;
    public Sprite sfxOffSprite;

    public Image vibrationIcon;
    public Sprite vibrationOnSprite;
    public Sprite vibrationOffSprite;

    void OnEnable()
    {
        UpdateIcons();
    }
    
    public void OnMusicButton()
    {
        if (!SoundManager.instance.IsMusicMuted())
        {
            SoundManager.instance.ForceMusicMute(true);
        }
        else
        {
            SoundManager.instance.ForceMusicMute(false);
        }

        UpdateIcons();
    }

    public void OnSfxButton()
    {
        if (!SoundManager.instance.IsSfxMuted())
        {
            SoundManager.instance.ForceSfxMute(true);
        }
        else
        {
            SoundManager.instance.ForceSfxMute(false);
        }

        UpdateIcons();
    }

    public void OnVoiceButton()
    {
        UpdateIcons();
    }

    public void OnVibrationButton()
    {
        SoundManager.instance.ToggleVibration();
        UpdateIcons();
    }

    public void OnNotificationButton()
    {
        UpdateIcons();
    }

    public void UpdateIcons()
    {
        musicIcon.sprite = SoundManager.instance.IsMusicMuted() ? musicOffSprite : musicOnSprite;
        sfxIcon.sprite = SoundManager.instance.IsSfxMuted() ? sfxOffSprite : sfxOnSprite;
        vibrationIcon.sprite = SoundManager.instance.IsVibrationMuted() ? vibrationOffSprite : vibrationOnSprite;
    }
}