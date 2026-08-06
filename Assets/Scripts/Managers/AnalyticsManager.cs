using System;
using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine;

[DefaultExecutionOrder(-150)]
public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    [Header("Debug")]
    [SerializeField]
    private bool showDebugLogs = true;

    [SerializeField]
    private int maximumQueuedEvents = 100;

    private bool firebaseReady;

    private readonly Queue<PendingAnalyticsEvent>
        pendingEvents = new();

    // Recommended Firebase event names.
    private const string LEVEL_START =
        "level_start";

    private const string LEVEL_END =
        "level_end";

    private const string TUTORIAL_BEGIN =
        "tutorial_begin";

    private const string TUTORIAL_COMPLETE =
        "tutorial_complete";

    private const string EARN_VIRTUAL_CURRENCY =
        "earn_virtual_currency";

    private const string SPEND_VIRTUAL_CURRENCY =
        "spend_virtual_currency";

    private const string SCREEN_VIEW =
        "screen_view";

    // Reusable custom event names.
    private const string AD_EVENT =
        "ad_event";

    private const string REVIVE_EVENT =
        "revive_event";

    private const string BOOSTER_EVENT =
        "booster_event";

    private const string PROGRESS_EVENT =
        "progress_event";

    private const string IAP_EVENT =
        "iap_event";

    private sealed class PendingAnalyticsEvent
    {
        public string eventName;
        public Action logAction;

        public PendingAnalyticsEvent(
            string eventName,
            Action logAction)
        {
            this.eventName = eventName;
            this.logAction = logAction;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        FirebaseInitializer.OnFirebaseInitializedEvent +=
            HandleFirebaseInitialized;
    }

    private void Start()
    {
        // Handles the case where Firebase initialized before
        // this manager subscribed to the event.
        if (FirebaseInitializer.IsInitialized)
        {
            HandleFirebaseInitialized();
        }
    }

    private void OnDisable()
    {
        FirebaseInitializer.OnFirebaseInitializedEvent -=
            HandleFirebaseInitialized;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void HandleFirebaseInitialized()
    {
        if (firebaseReady)
            return;

        firebaseReady = true;

        DebugLog(
            "Firebase Analytics is ready. " +
            $"Sending {pendingEvents.Count} queued events."
        );

        FlushQueuedEvents();
    }

    // ==========================================
    // LEVEL EVENTS
    // ==========================================

    public void LogLevelStart(
        int levelNumber,
        string worldName,
        int difficulty)
    {
        string levelName =
            "level_" + levelNumber;

        string cleanWorld =
            CleanValue(worldName);

        RunOrQueue(
            LEVEL_START,
            () =>
            {
                FirebaseAnalytics.LogEvent(
                    LEVEL_START,
                    new Parameter(
                        "level_name",
                        levelName
                    ),
                    new Parameter(
                        "level_number",
                        (long)levelNumber
                    ),
                    new Parameter(
                        "world_name",
                        cleanWorld
                    ),
                    new Parameter(
                        "difficulty",
                        (long)difficulty
                    )
                );
            }
        );
    }

    public void LogLevelEnd(
        int levelNumber,
        bool success,
        string reason,
        float durationSeconds,
        string worldName)
    {
        string levelName =
            "level_" + levelNumber;

        string cleanReason =
            CleanValue(reason);

        string cleanWorld =
            CleanValue(worldName);

        RunOrQueue(
            LEVEL_END,
            () =>
            {
                FirebaseAnalytics.LogEvent(
                    LEVEL_END,
                    new Parameter(
                        "level_name",
                        levelName
                    ),
                    new Parameter(
                        "level_number",
                        (long)levelNumber
                    ),
                    new Parameter(
    "success",
    success ? 1L : 0L
),
                    new Parameter(
                        "reason",
                        cleanReason
                    ),
                    new Parameter(
                        "duration_seconds",
                        (double)durationSeconds
                    ),
                    new Parameter(
                        "world_name",
                        cleanWorld
                    )
                );
            }
        );
    }

    // ==========================================
    // TUTORIAL EVENTS
    // ==========================================

    public void LogTutorialBegin(
        string tutorialName)
    {
        string cleanTutorial =
            CleanValue(tutorialName);

        RunOrQueue(
            TUTORIAL_BEGIN,
            () =>
            {
                FirebaseAnalytics.LogEvent(
                    TUTORIAL_BEGIN,
                    new Parameter(
                        "tutorial_name",
                        cleanTutorial
                    )
                );
            }
        );
    }

    public void LogTutorialComplete(
        string tutorialName)
    {
        string cleanTutorial =
            CleanValue(tutorialName);

        RunOrQueue(
            TUTORIAL_COMPLETE,
            () =>
            {
                FirebaseAnalytics.LogEvent(
                    TUTORIAL_COMPLETE,
                    new Parameter(
                        "tutorial_name",
                        cleanTutorial
                    )
                );
            }
        );
    }

    // ==========================================
    // VIRTUAL CURRENCY EVENTS
    // ==========================================

    public void LogCurrencyEarned(
        string currencyName,
        int amount,
        string source)
    {
        string cleanCurrency =
            CleanValue(currencyName);

        string cleanSource =
            CleanValue(source);

        RunOrQueue(
            EARN_VIRTUAL_CURRENCY,
            () =>
            {
                FirebaseAnalytics.LogEvent(
                    EARN_VIRTUAL_CURRENCY,
                    new Parameter(
                        "virtual_currency_name",
                        cleanCurrency
                    ),
                    new Parameter(
                        "value",
                        (long)amount
                    ),
                    new Parameter(
                        "source",
                        cleanSource
                    )
                );
            }
        );
    }

    public void LogCurrencySpent(
        string currencyName,
        int amount,
        string itemName)
    {
        string cleanCurrency =
            CleanValue(currencyName);

        string cleanItem =
            CleanValue(itemName);

        RunOrQueue(
            SPEND_VIRTUAL_CURRENCY,
            () =>
            {
                FirebaseAnalytics.LogEvent(
                    SPEND_VIRTUAL_CURRENCY,
                    new Parameter(
                        "virtual_currency_name",
                        cleanCurrency
                    ),
                    new Parameter(
                        "value",
                        (long)amount
                    ),
                    new Parameter(
                        "item_name",
                        cleanItem
                    )
                );
            }
        );
    }

    // ==========================================
    // AD EVENTS
    // ==========================================

    public void LogAdEvent(
        string action,
        string adType,
        string placement,
        int levelNumber = -1,
        string result = "none")
    {
        string cleanAction =
            CleanValue(action);

        string cleanAdType =
            CleanValue(adType);

        string cleanPlacement =
            CleanValue(placement);

        string cleanResult =
            CleanValue(result);

        RunOrQueue(
            AD_EVENT,
            () =>
            {
                FirebaseAnalytics.LogEvent(
                    AD_EVENT,
                    new Parameter(
                        "action",
                        cleanAction
                    ),
                    new Parameter(
                        "ad_type",
                        cleanAdType
                    ),
                    new Parameter(
                        "placement",
                        cleanPlacement
                    ),
                    new Parameter(
                        "level_number",
                        (long)levelNumber
                    ),
                    new Parameter(
                        "result",
                        cleanResult
                    )
                );
            }
        );
    }

    // ==========================================
    // REVIVE EVENTS
    // ==========================================

    public void LogReviveEvent(
     string action,
     string method,
     int levelNumber,
     string result = "none")
    {
        string cleanAction =
            CleanValue(action);

        string cleanMethod =
            CleanValue(method);

        string cleanResult =
            CleanValue(result);

        RunOrQueue(
            REVIVE_EVENT,
            () =>
            {
                FirebaseAnalytics.LogEvent(
                    REVIVE_EVENT,
                    new Parameter(
                        "action",
                        cleanAction
                    ),
                    new Parameter(
                        "method",
                        cleanMethod
                    ),
                    new Parameter(
                        "level_number",
                        (long)levelNumber
                    ),
                    new Parameter(
                        "result",
                        cleanResult
                    )
                );
            }
        );
    }

    // ==========================================
    // BOOSTER EVENTS
    // ==========================================

    public void LogBoosterEvent(
        string action,
        string boosterType,
        int amount,
        int levelNumber,
        string source = "gameplay")
    {
        string cleanAction =
            CleanValue(action);

        string cleanBooster =
            CleanValue(boosterType);

        string cleanSource =
            CleanValue(source);

        RunOrQueue(
            BOOSTER_EVENT,
            () =>
            {
                FirebaseAnalytics.LogEvent(
                    BOOSTER_EVENT,
                    new Parameter(
                        "action",
                        cleanAction
                    ),
                    new Parameter(
                        "booster_type",
                        cleanBooster
                    ),
                    new Parameter(
                        "amount",
                        (long)amount
                    ),
                    new Parameter(
                        "level_number",
                        (long)levelNumber
                    ),
                    new Parameter(
                        "source",
                        cleanSource
                    )
                );
            }
        );
    }

    // ==========================================
    // PROGRESSION EVENTS
    // ==========================================

    public void LogProgressEvent(
        string action,
        string progressType,
        string worldName,
        int destinationIndex,
        int levelNumber)
    {
        string cleanAction =
            CleanValue(action);

        string cleanProgressType =
            CleanValue(progressType);

        string cleanWorld =
            CleanValue(worldName);

        RunOrQueue(
            PROGRESS_EVENT,
            () =>
            {
                FirebaseAnalytics.LogEvent(
                    PROGRESS_EVENT,
                    new Parameter(
                        "action",
                        cleanAction
                    ),
                    new Parameter(
                        "progress_type",
                        cleanProgressType
                    ),
                    new Parameter(
                        "world_name",
                        cleanWorld
                    ),
                    new Parameter(
                        "destination_index",
                        (long)destinationIndex
                    ),
                    new Parameter(
                        "level_number",
                        (long)levelNumber
                    )
                );
            }
        );
    }

    // ==========================================
    // IAP EVENTS
    // ==========================================

    public void LogIapEvent(
        string action,
        string productId,
        string productType,
        string failureReason = "none")
    {
        string cleanAction =
            CleanValue(action);

        string cleanProductId =
            CleanValue(productId);

        string cleanProductType =
            CleanValue(productType);

        string cleanFailureReason =
            CleanValue(failureReason);

        RunOrQueue(
            IAP_EVENT,
            () =>
            {
                FirebaseAnalytics.LogEvent(
                    IAP_EVENT,
                    new Parameter(
                        "action",
                        cleanAction
                    ),
                    new Parameter(
                        "product_id",
                        cleanProductId
                    ),
                    new Parameter(
                        "product_type",
                        cleanProductType
                    ),
                    new Parameter(
                        "failure_reason",
                        cleanFailureReason
                    )
                );
            }
        );
    }

    // ==========================================
    // SCREEN EVENTS
    // ==========================================

    public void LogScreenView(
        string screenName)
    {
        string cleanScreen =
            CleanValue(screenName);

        RunOrQueue(
            SCREEN_VIEW,
            () =>
            {
                FirebaseAnalytics.LogEvent(
                    SCREEN_VIEW,
                    new Parameter(
                        "screen_name",
                        cleanScreen
                    ),
                    new Parameter(
                        "screen_class",
                        "unity_ui"
                    )
                );
            }
        );
    }

    // ==========================================
    // INTERNAL LOGGING
    // ==========================================

    private void RunOrQueue(
        string eventName,
        Action logAction)
    {
        if (logAction == null)
            return;

        if (firebaseReady ||
            FirebaseInitializer.IsInitialized)
        {
            firebaseReady = true;
            SendEvent(eventName, logAction);
            return;
        }

        if (pendingEvents.Count >=
            maximumQueuedEvents)
        {
            PendingAnalyticsEvent removed =
                pendingEvents.Dequeue();

            Debug.LogWarning(
                "[Analytics] Queue full. Removed oldest event: " +
                removed.eventName
            );
        }

        pendingEvents.Enqueue(
            new PendingAnalyticsEvent(
                eventName,
                logAction
            )
        );

        DebugLog(
            "Queued event: " + eventName
        );
    }

    private void FlushQueuedEvents()
    {
        while (pendingEvents.Count > 0)
        {
            PendingAnalyticsEvent pending =
                pendingEvents.Dequeue();

            SendEvent(
                pending.eventName,
                pending.logAction
            );
        }
    }

    private void SendEvent(
        string eventName,
        Action logAction)
    {
        try
        {
            logAction.Invoke();

            DebugLog(
                "Sent event: " + eventName
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[Analytics] Failed to send event: " +
                eventName
            );

            Debug.LogException(exception);
        }
    }

    private string CleanValue(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "unknown";

        string cleaned =
            value.Trim()
                .ToLowerInvariant()
                .Replace(" ", "_")
                .Replace("-", "_");

        // Firebase string parameter values should be kept short.
        if (cleaned.Length > 100)
        {
            cleaned = cleaned.Substring(0, 100);
        }

        return cleaned;
    }

    private void DebugLog(
        string message)
    {
        if (!showDebugLogs)
            return;

        Debug.Log(
            "<color=#57C785>[Analytics] " +
            message +
            "</color>"
        );
    }
}