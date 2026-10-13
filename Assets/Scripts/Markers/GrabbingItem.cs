using UnityEngine;

/// <summary>
/// Обозначает объект как доступный для захвата.
///
/// Крепится на: GameObject, который должен поддерживать захват.
///
/// Используется:
/// - Grab — для получения Rigidbody захватываемого объекта.
/// 
/// </summary>

[RequireComponent(typeof(InteractiveObject))]
public class GrabbingItem : MonoBehaviour
{
    public InteractiveObject InteractiveObject { get; private set; }

    [field: SerializeField] public bool CanBeGrab { get; set; } = true;
}
