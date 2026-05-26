using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    private int currentLevelIndex;

    [Header("Next Level Transition")]
    public UIParticle nextLevelParticles;
    public float delayBeforeNextLevel = 2f;
    public Button nextLevelButton;

    [SerializeField]
    private LevelDatabase levelDatabase;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //PlayerPrefs.DeleteAll();
        currentLevelIndex = PlayerPrefs.GetInt("Level", 0);

        levelDatabase =
        Resources.Load<LevelDatabase>(
        "LevelDatabase"
        );

        LoadLevel(currentLevelIndex);
        Debug.Log("Saved Level: " + currentLevelIndex);
    }

    public void LoadLevel(int index)
    {
        currentLevelIndex = index;

        // HANDMADE LEVELS
        if (index < levelDatabase.levels.Count)
        {
            LevelData handmadeLevel = levelDatabase.levels[index];

            ProceduralLevelData data = new ProceduralLevelData();

            data.layerLayouts = new List<string[]>();

            foreach (ShapeData shape in handmadeLevel.layers)
            {
                data.layerLayouts.Add(
                    shape.layout
                );
            }

            string[] biggestLayout = data.layerLayouts[0];

            data.layout = biggestLayout;
            data.rows = biggestLayout.Length;
            data.cols = biggestLayout[0].Length;

            BoardGenerator.instance.SetProceduralLevel(data);
            GameManager.instance.UpdateLevelText(index);
            Debug.Log("Loaded Handmade Level: " + index);

            return;
        }

        // PROCEDURAL INFINITE
        ProceduralLevelData levelData =
            ProceduralLevelGenerator.instance
                .GenerateLevel(index);

        if (levelData == null)
        {
            Debug.LogError(
                "Level data is null"
            );

            return;
        }

        BoardGenerator.instance.SetProceduralLevel(levelData);
        GameManager.instance.UpdateLevelText(index);
        Debug.Log("Loaded Infinite Level: " + index);
    }

    public void NextLevel()
    {
        StartCoroutine(NextLevelRoutine());
    }

    private IEnumerator NextLevelRoutine()
    {
        nextLevelButton.interactable = false;

        yield return new WaitForSeconds(2f);

        currentLevelIndex++;

        PlayerPrefs.SetInt("Level", currentLevelIndex);
        PlayerPrefs.Save();

        UIManager.Instance.HidePopup(ScreenType.LevelCompleted);

        LoadLevel(currentLevelIndex);
        GameManager.instance.ResetLevelState();

        nextLevelButton.interactable = true;
    }
}