using System.Collections.Generic;
using UnityEngine;

public class TutorialAnalyticsTracker : MonoBehaviour
{
    public static TutorialAnalyticsTracker Instance
    {
        get;
        private set;
    }

    [Header("Debug")]
    [SerializeField]
    private bool showDebugLogs = true;

    private string activeTutorialName;

    // Prevent duplicate begin/complete calls during
    // the same game session.
    private readonly HashSet<string>
        startedTutorials = new();

    private readonly HashSet<string>
        completedTutorials = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void BeginTutorial(
        string tutorialName)
    {
        tutorialName =
            CleanTutorialName(tutorialName);

        if (string.IsNullOrEmpty(tutorialName))
        {
            Debug.LogWarning(
                "[Tutorial Analytics] " +
                "Tutorial name is empty."
            );

            return;
        }

        activeTutorialName = tutorialName;

        // Prevent duplicate tutorial_begin events.
        if (!startedTutorials.Add(tutorialName))
            return;

        AnalyticsManager.Instance
            ?.LogTutorialBegin(tutorialName);

        DebugLog(
            "Started: " + tutorialName
        );
    }

    public void CompleteTutorial(
        string tutorialName)
    {
        tutorialName =
            CleanTutorialName(tutorialName);

        if (string.IsNullOrEmpty(tutorialName))
            return;

        // Make sure complete never exists without begin.
        if (!startedTutorials.Contains(tutorialName))
        {
            BeginTutorial(tutorialName);
        }

        // Prevent duplicate tutorial_complete events.
        if (!completedTutorials.Add(tutorialName))
            return;

        AnalyticsManager.Instance
            ?.LogTutorialComplete(tutorialName);

        DebugLog(
            "Completed: " + tutorialName
        );

        if (activeTutorialName == tutorialName)
        {
            activeTutorialName = string.Empty;
        }
    }

    public void CompleteCurrentTutorial()
    {
        if (string.IsNullOrEmpty(activeTutorialName))
        {
            Debug.LogWarning(
                "[Tutorial Analytics] " +
                "No active tutorial to complete."
            );

            return;
        }

        CompleteTutorial(activeTutorialName);
    }

    public void CancelCurrentTutorial()
    {
        DebugLog(
            "Cancelled: " + activeTutorialName
        );

        activeTutorialName = string.Empty;
    }

    private string CleanTutorialName(
        string tutorialName)
    {
        if (string.IsNullOrWhiteSpace(tutorialName))
            return string.Empty;

        return tutorialName
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "_")
            .Replace("-", "_");
    }

    private void DebugLog(string message)
    {
        if (!showDebugLogs)
            return;

        Debug.Log(
            "<color=#C77DFF>" +
            "[Tutorial Analytics] " +
            message +
            "</color>"
        );
    }
}