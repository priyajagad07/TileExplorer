using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [SerializeField] private Sound[] sounds;
    private Dictionary<SoundName, AudioClip> soundDict;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;
    private bool isMusicMuted = false;
    private bool isSfxMuted = false;
    private bool isVoiceMuted = false;
    private bool isVibrationMuted = false;
    private bool isNotificationMuted = false;
    public bool IsMusicMuted() => isMusicMuted;
    public bool IsSfxMuted() => isSfxMuted;
    public bool IsVolumeMuted() => isVoiceMuted;
    public bool IsVibrationMuted() => isVibrationMuted;
    public bool IsNotificationMuted() => isNotificationMuted;

    void Start()
    {
        if (soundDict.ContainsKey(SoundName.BackGround))
        {
            PlaySound(SoundName.BackGround);
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        soundDict = new Dictionary<SoundName, AudioClip>();

        foreach (var s in sounds)
        {
            if (!soundDict.ContainsKey(s.name))
            {
                soundDict.Add(s.name, s.audio);
            }
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!musicSource.isPlaying)
        {
            PlayMusic(SoundName.BackGround);
        }
    }

    public void PlaySound(SoundName name)
    {
        Debug.Log("Playing sound: " + name);

        if (soundDict.ContainsKey(name) && !isSfxMuted)
        {
            sfxSource.PlayOneShot(soundDict[name], sfxVolume);
        }
    }

    public void PlayMusic(SoundName name)
    {
        if (soundDict.ContainsKey(name))
        {
            musicSource.clip = soundDict[name];
            musicSource.loop = true;
            musicSource.volume = isMusicMuted ? 0 : musicVolume;
            musicSource.Play();
        }
    }

    public void MuteAll(bool val)
    {
        sfxSource.mute = val;
        musicSource.mute = val;
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = value;

        if (!isMusicMuted)
            musicSource.volume = value;
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = value;

        if (!isSfxMuted)
            sfxSource.volume = value;
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;

        if (isMusicMuted)
        {
            musicSource.volume = 0;
        }
        else
        {
            musicSource.volume = musicVolume;
        }
    }

    public void ToggleSfx()
    {
        isSfxMuted = !isSfxMuted;

        if (isSfxMuted)
        {
            sfxSource.volume = 0;
        }
        else
        {
            sfxSource.volume = sfxVolume;
        }
    }

    public void ForceMusicMute(bool val)
    {
        isMusicMuted = val;

        if (val)
        {
            musicSource.volume = 0;
        }
        else
        {
            musicSource.volume = musicVolume;
        }
    }

    public void ForceSfxMute(bool val)
    {
        isSfxMuted = val;

        if (val)
        {
            sfxSource.volume = 0;
        }
        else
        {
            sfxSource.volume = sfxVolume;
        }
    }
}

[System.Serializable]
public class Sound
{
    public SoundName name;
    public AudioClip audio;
}