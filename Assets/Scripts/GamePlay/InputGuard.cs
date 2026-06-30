using UnityEngine;

public class InputGuard : MonoBehaviour
{
    public static InputGuard Instance;

    private bool locked;

    void Awake()
    {
        Instance = this;
    }

    public bool IsLocked => locked;

    public bool TryLock()
    {
        if (locked)
            return false;

        locked = true;
        return true;
    }

    public void Unlock()
    {
        locked = false;
    }
}