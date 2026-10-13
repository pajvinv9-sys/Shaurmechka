using UnityEngine;
/// <summary>
/// Маркер земли.
/// Юзается в методе прыжка персонажа
/// </summary>
[RequireComponent (typeof(Collider))]
public class Ground : MonoBehaviour
{
    public InteractiveObject InteractiveObject { get; private set; }
}
