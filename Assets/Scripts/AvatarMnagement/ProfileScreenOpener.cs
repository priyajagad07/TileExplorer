using DG.Tweening;
using UnityEngine;

/// <summary>
/// Attach to the ProfileScreen root — the object that is
/// SetActive(true) when the player opens profile from Home.
/// Plays pending avatar unlock bounces each time the screen opens.
/// </summary>
public class ProfileScreenOpener : MonoBehaviour
{
    [SerializeField]
    private float unlockAnimDelay = 0.25f;

    private void OnEnable()
    {
        DOVirtual.DelayedCall(
            unlockAnimDelay,
            PlayUnlockIfReady
        ).SetUpdate(true);
    }

    private void PlayUnlockIfReady()
    {
        if (!isActiveAndEnabled)
            return;

        if (AvatarManager.Instance == null)
            return;

        AvatarManager.Instance.OnProfileScreenOpened();
    }
}
