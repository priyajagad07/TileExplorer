using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Solo.MOST_IN_ONE;

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

    // --- UPDATED: Smart Identical Pitch Tracking (No Timer!) ---
    [Header("Dynamic Pitch Settings")]
    [SerializeField] private float maxPitch = 1.6f;
    [SerializeField] private float pitchStep = 0.3f;

    private float currentTilePitch = 1.0f;
    private int lastClickedTileID = -1;
    private int sameTileClickCount = 0;
    // -------------------------------------------

    public bool IsMusicMuted() => isMusicMuted;
    public bool IsSfxMuted() => isSfxMuted;
    public bool IsVolumeMuted() => isVoiceMuted;
    public bool IsVibrationMuted() => isVibrationMuted;
    public bool IsNotificationMuted() => isNotificationMuted;

    void Start()
    {
        if (soundDict.ContainsKey(SoundName.BackGround))
        {
            PlayMusic(SoundName.BackGround);
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
        LoadSettings();
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

    // --- THE NEW SMART CLICK METHOD ---
    public void PlayTileClick(int tileID)
    {
        if (!soundDict.ContainsKey(SoundName.TileClick) || isSfxMuted) return;

        // CHANGED: Now we ONLY check if it's the exact same tile type! No timer needed.
        if (tileID == lastClickedTileID)
        {
            sameTileClickCount++;
            currentTilePitch = 1.0f + (sameTileClickCount * pitchStep);
            currentTilePitch = Mathf.Min(currentTilePitch, maxPitch); 
        }
        else
        {
            // Reset if they clicked a different tile
            ResetPitchTracker();
            lastClickedTileID = tileID; 
        }

        sfxSource.pitch = currentTilePitch;
        
        Debug.Log($"🎵 Clicked Tile ID: {tileID} | Same Tile Chain: {sameTileClickCount} | Pitch: {currentTilePitch}");

        sfxSource.PlayOneShot(soundDict[SoundName.TileClick], sfxVolume);
    }

    // Call this when a match of 3 happens!
    public void ResetPitchTracker()
    {
        currentTilePitch = 1.0f;
        sameTileClickCount = 0;
        lastClickedTileID = -1;
    }
    // -----------------------------------

    public void PlaySound(SoundName name)
    {
        if (soundDict.ContainsKey(name) && !isSfxMuted)
        {
            sfxSource.pitch = 1.0f; // Force normal pitch for all other sounds
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

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;
        musicSource.volume = isMusicMuted ? 0 : musicVolume;
        SaveSettings();
    }

    public void ToggleSfx()
    {
        isSfxMuted = !isSfxMuted;
        sfxSource.volume = isSfxMuted ? 0 : sfxVolume;
        SaveSettings();
    }

    public void ToggleVibration()
    {
        isVibrationMuted = !isVibrationMuted;
        SaveSettings();
    }

    public void ForceMusicMute(bool val)
    {
        isMusicMuted = val;
        musicSource.volume = val ? 0 : musicVolume;
        SaveSettings();
    }

    public void ForceSfxMute(bool val)
    {
        isSfxMuted = val;
        sfxSource.volume = val ? 0 : sfxVolume;
        SaveSettings();
    }

    void LoadSettings()
    {
        if (SaveManager.instance == null) return; 

        isMusicMuted = SaveManager.instance.data.musicMuted == 1;
        isSfxMuted = SaveManager.instance.data.sfxMuted == 1;
        musicVolume = SaveManager.instance.data.musicVolume;
        sfxVolume = SaveManager.instance.data.sfxVolume;
        isVibrationMuted = SaveManager.instance.data.vibrationMuted == 1;

        musicSource.volume = isMusicMuted ? 0 : musicVolume;
        sfxSource.volume = isSfxMuted ? 0 : sfxVolume;
    }

    void SaveSettings()
    {
        SaveManager.instance.data.musicMuted = isMusicMuted ? 1 : 0;
        SaveManager.instance.data.sfxMuted = isSfxMuted ? 1 : 0;
        SaveManager.instance.data.musicVolume = musicVolume;
        SaveManager.instance.data.sfxVolume = sfxVolume;
        SaveManager.instance.data.vibrationMuted = isVibrationMuted ? 1 : 0;
        SaveManager.instance.SaveData();
    }

    public void PlayHaptic(MOST_HapticFeedback.HapticTypes type)
    {
        if (isVibrationMuted) return;
        MOST_HapticFeedback.Generate(type);
    }
}

[System.Serializable]
public class Sound
{
    public SoundName name;
    public AudioClip audio;
}