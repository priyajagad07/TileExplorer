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

    public Image voiceIcon;
    public Sprite voiceOnSprite;
    public Sprite voiceOffSprite;

    public Image vibrationIcon;
    public Sprite vibrationOnSprite;
    public Sprite vibrationOffSprite;

    public Image notificationIcon;
    public Sprite notificationOnSprite;
    public Sprite notificationOffSprite;

    void Start()
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
        UpdateIcons();
    }

    public void OnNotificationButton()
    {
        UpdateIcons();
    }

    void UpdateIcons()
    {
        musicIcon.sprite = SoundManager.instance.IsMusicMuted() ? musicOffSprite : musicOnSprite;
        sfxIcon.sprite = SoundManager.instance.IsSfxMuted() ? sfxOffSprite : sfxOnSprite;
        voiceIcon.sprite = SoundManager.instance.IsVolumeMuted() ? voiceOffSprite : voiceOnSprite;
        vibrationIcon.sprite = SoundManager.instance.IsVibrationMuted() ? vibrationOffSprite : vibrationOnSprite;
        notificationIcon.sprite = SoundManager.instance.IsNotificationMuted() ? notificationOffSprite : notificationOnSprite;
    }
}