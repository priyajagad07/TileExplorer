using Firebase;
using Firebase.Extensions;
using UnityEngine;
using Firebase.Analytics;

[DefaultExecutionOrder(-200)]
public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseInitializer Instance { get; private set; }

    public static bool IsInitialized { get; private set; }

    public delegate void OnFirebaseInitialized();
    public static event OnFirebaseInitialized
        OnFirebaseInitializedEvent;

    private FirebaseApp app;

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

    private void Start()
    {
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        Debug.Log(
            "<color=green>Firebase initialization started...</color>"
        );

        FirebaseApp.CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError(
                        "Firebase initialization was cancelled."
                    );
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError(
                        "Firebase dependency check failed."
                    );

                    Debug.LogException(task.Exception);
                    return;
                }

                DependencyStatus dependencyStatus =
                    task.Result;

                if (dependencyStatus !=
                    DependencyStatus.Available)
                {
                    Debug.LogError(
                        "Could not resolve all Firebase " +
                        "dependencies: " +
                        dependencyStatus
                    );

                    return;
                }

                app = FirebaseApp.DefaultInstance;
                IsInitialized = true;

                // FirebaseAnalytics.LogEvent("firebase_test_event", new Parameter("app_version", Application.version),
                //                                                   new Parameter("platform", Application.platform.ToString()));

                //Debug.Log("<color=cyan>" + "Triggered Analytics event: analytics_test_event" + "</color>");
                Debug.Log("<color=green>" + "Firebase initialization completed!" + "</color>");

                OnFirebaseInitializedEvent?.Invoke();
            });
    }
}