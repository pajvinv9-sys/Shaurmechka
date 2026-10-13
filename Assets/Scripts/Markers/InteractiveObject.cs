using UnityEngine;
/// <summary>
/// Компонент-маркер для объектов, с которыми можно взаимодействовать
/// Для примера смотри GrabbingItem.cs и LaunchBeam.cs
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class InteractiveObject : MonoBehaviour
{
    public Rigidbody Rigidbody { get; private set; }
    public Collider Collider { get; private set; }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Collider = GetComponent<Collider>();
    }
}
