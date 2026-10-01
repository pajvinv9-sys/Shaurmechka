using UnityEngine;

/// <summary>
/// Обозначает объект как доступный для захвата.
///
/// Крепится на: GameObject, который должен поддерживать захват.
///
/// Основная логика:
/// - Получает Rigidbody объекта при запуске.
/// - Если Rigidbody отсутствует, автоматически добавляет его.
///
/// Используется:
/// - Grab — для получения Rigidbody захватываемого объекта.
/// </summary>
public class GrabbingItem : MonoBehaviour
{
    public Rigidbody Rb { get; private set; }
    public InteractiveObject Io { get; private set; }

    void Start()
    {
        Rb = GetComponent<Rigidbody>();

        if (Rb == null)
            Rb = gameObject.AddComponent<Rigidbody>();

        Io = GetComponent<InteractiveObject>();

        if (Io == null)
            Io = gameObject.AddComponent<InteractiveObject>();
    }
}
