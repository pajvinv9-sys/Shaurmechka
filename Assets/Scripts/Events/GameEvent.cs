using UnityEngine;

public abstract class GameEvent : MonoBehaviour
{
    public abstract void Activate();
    public abstract void Deactivate();
    public static bool IsActive { get; protected set; } = false;
}
