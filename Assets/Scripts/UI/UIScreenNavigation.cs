// using UnityEngine;

// public class UIScreenNavigation : MonoBehaviour
// {
//     [Header("Screen Container")]
//     [SerializeField] private RectTransform screensContainer;

//     [Header("Screen Order")]
//     [Tooltip("0 Shop, 1 Streak, 2 Home, 3 Map, 4 Trophy")]
//     [SerializeField] private UIScreenAnimation[] screens;

//     [Header("Starting Screen")]
//     [SerializeField] private int startingScreenIndex = 2;

//     [Header("Transition")]
//     [SerializeField] private float extraSlideDistance = 50f;

//     private int currentScreenIndex;
//     private bool isTransitioning;

//     private void Start()
//     {
//         if (screens == null || screens.Length == 0)
//         {
//             Debug.LogError("No screens assigned to UIScreenNavigation.");
//             return;
//         }

//         currentScreenIndex = Mathf.Clamp(
//             startingScreenIndex,
//             0,
//             screens.Length - 1
//         );

//         for (int i = 0; i < screens.Length; i++)
//         {
//             if (screens[i] == null)
//                 continue;

//             screens[i].SetVisibleInstant(i == currentScreenIndex);
//         }
//     }

//     public void OpenScreen(int targetScreenIndex)
//     {
//         if (isTransitioning)
//             return;

//         if (targetScreenIndex < 0 ||
//             targetScreenIndex >= screens.Length)
//         {
//             Debug.LogWarning(
//                 $"Invalid screen index: {targetScreenIndex}"
//             );

//             return;
//         }

//         if (targetScreenIndex == currentScreenIndex)
//             return;

//         UIScreenAnimation currentScreen =
//             screens[currentScreenIndex];

//         UIScreenAnimation nextScreen =
//             screens[targetScreenIndex];

//         if (currentScreen == null || nextScreen == null)
//         {
//             Debug.LogError("Screen reference is missing.");
//             return;
//         }

//         isTransitioning = true;

//         float slideDistance = GetSlideDistance();

//         bool destinationIsOnRight =
//             targetScreenIndex > currentScreenIndex;

//         HorizontalSide nextScreenStartSide;
//         HorizontalSide currentScreenExitSide;

//         if (destinationIsOnRight)
//         {
//             // Example: Home -> Map
//             // Map enters from right.
//             // Home exits to left.
//             nextScreenStartSide = HorizontalSide.Right;
//             currentScreenExitSide = HorizontalSide.Left;
//         }
//         else
//         {
//             // Example: Home -> Streak
//             // Streak enters from left.
//             // Home exits to right.
//             nextScreenStartSide = HorizontalSide.Left;
//             currentScreenExitSide = HorizontalSide.Right;
//         }

//         currentScreen.HideToSide(
//             currentScreenExitSide,
//             slideDistance
//         );

//         nextScreen.ShowFromSide(
//             nextScreenStartSide,
//             slideDistance,
//             () =>
//             {
//                 currentScreenIndex = targetScreenIndex;
//                 isTransitioning = false;
//             }
//         );
//     }

//     private float GetSlideDistance()
//     {
//         Canvas.ForceUpdateCanvases();

//         if (screensContainer != null &&
//             screensContainer.rect.width > 0f)
//         {
//             return screensContainer.rect.width +
//                    extraSlideDistance;
//         }

//         return Screen.width + extraSlideDistance;
//     }

//     // These can be directly assigned to button OnClick events.

//     public void OpenShop()
//     {
//         OpenScreen(0);
//     }

//     public void OpenStreak()
//     {
//         OpenScreen(1);
//     }

//     public void OpenHome()
//     {
//         OpenScreen(2);
//     }

//     public void OpenMap()
//     {
//         OpenScreen(3);
//     }

//     public void OpenTrophy()
//     {
//         OpenScreen(4);
//     }
// }