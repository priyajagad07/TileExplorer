using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    private int currentLevelIndex;

    [Header("Next Level Transition")]
    public UIParticle nextLevelParticles;
    public Button nextLevelButton;
    private bool isLoadingNextLevel = false;

    [SerializeField] private LevelDatabase levelDatabase;
    [SerializeField] private float nextLevelParticleDuration = 1.2f;
    public bool shouldPlaySpawnAnimation;
    public bool loadLevelSilently = false;
    [SerializeField]
    private TMP_Text nextLevelText;
    public bool skipMapRefresh;

    void Awake()
    {
        instance = this;
    }

    void Start()
    { 
        currentLevelIndex = SaveManager.instance.data.level;
        levelDatabase = Resources.Load<LevelDatabase>("LevelDatabase");

        LoadLevel(currentLevelIndex);
        Debug.Log("Saved Level: " + currentLevelIndex);
    }

    public void LoadLevel(int index)
    {
        Debug.Log("=== LOAD LEVEL CALLED ===");
        Debug.Log("Loading Level Index: " + index);

        currentLevelIndex = index;
        CountryData country = CountryManager.Instance.GetCountryForLevel(index + 1);

        if (country != null)
        {
            Debug.Log("Country found: " + country.countryName);
            BackgroundManager.Instance.UpdateBackgrounds(country, index + 1);

            if (!skipMapRefresh)
            {
                MapManager.instance?.RefreshMap();
            }

            skipMapRefresh = false;
        }
        else
        {
            Debug.LogWarning("No country found for level: " + (index + 1));
        }

        if (index < levelDatabase.levels.Count)
        {
            LevelData handmadeLevel = levelDatabase.levels[index];
            ProceduralLevelData data = new ProceduralLevelData();
            data.layerLayouts = new List<string[]>();

            foreach (ShapeData shape in handmadeLevel.layers)
            {
                data.layerLayouts.Add(shape.layout);
            }

            string[] biggestLayout = data.layerLayouts[0];
            data.layout = biggestLayout;
            data.rows = biggestLayout.Length;
            data.cols = biggestLayout[0].Length;
            
            data.stackStyle = handmadeLevel.stackStyle;
            data.stackOffsetX = handmadeLevel.stackOffsetX;
            data.stackOffsetY = handmadeLevel.stackOffsetY;

            BoardGenerator.instance.SetProceduralLevel(data);
            GameManager.instance.UpdateLevelText(index);
            Debug.Log("Loaded Handmade Level: " + index);
            return;
        }

        ProceduralLevelData levelData = ProceduralLevelGenerator.instance.GenerateLevel(index);

        if (levelData == null)
        {
            Debug.LogError("Level data is null");
            return;
        }

        BoardGenerator.instance.SetProceduralLevel(levelData);
        GameManager.instance.UpdateLevelText(index);
        Debug.Log("Loaded Infinite Level: " + index);
    }

    public void NextLevel(bool playParticle = true)
    {
        Debug.Log("=== NEXT LEVEL CALLED ===");

        if (isLoadingNextLevel)
        {
            Debug.LogWarning("Already loading next level - ignoring call");
            return;
        }

        StartCoroutine(NextLevelRoutine(playParticle));
    }

    private IEnumerator NextLevelRoutine(bool playParticle)
    {
        isLoadingNextLevel = true;
        nextLevelButton.interactable = false;

        if (playParticle && nextLevelParticles != null)
        {
            nextLevelParticles.Play();
            yield return new WaitForSeconds(nextLevelParticleDuration);
        }

        currentLevelIndex++;
        Debug.Log("Moving to next level: " + currentLevelIndex);

        SaveManager.instance.data.level = currentLevelIndex;
        SaveManager.instance.SaveData();

        UIManager.Instance.HidePopup(ScreenType.LevelCompleted);
        LoadLevel(currentLevelIndex);

        if (!loadLevelSilently)
        {
            DOVirtual.DelayedCall(0.1f, () =>
            {
                if (BoardSpawner.instance != null)
                {
                    BoardSpawner.instance.PlaySpawnAnimation();
                }
            });
        }

        loadLevelSilently = false;
        GameManager.instance.ResetLevelState();
        nextLevelButton.interactable = true;
        isLoadingNextLevel = false;
    }

    public void UpdateNextButtonText()
    {
        int currentLevel = SaveManager.instance.data.level + 1;

        CountryData currentCountry = BackgroundManager.Instance.GetCurrentCountry();
        CountryData nextCountry = CountryManager.Instance.GetCountryForLevel(currentLevel + 1);

        bool countryChanging = nextCountry != currentCountry;
        bool unlockingDestination = BackgroundManager.Instance.IsNextDestinationUnlock();

        nextLevelText.text = (unlockingDestination || countryChanging) ? "Next Destination" : "Next Level";
    }
}