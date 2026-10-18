using System.Collections;
using UnityEngine;

/// <summary>
/// Базовый класс для интерактивных объектов.
/// Используется LaunchBeam для обнаружения объектов, с которыми можно взаимодействовать.
///
/// Наследники должны реализовать логику реакции на обнаружение объекта.
/// </summary>

public abstract class InteractiveObject : MonoBehaviour
{
    // Компоненты физики, необходимые для взаимодействия с объектом.
    public Rigidbody Rigidbody { get; private set; }
    public Collider Collider { get; private set; }

    protected virtual void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Collider = GetComponent<Collider>();
    }

    /// <summary>
    /// Вызывается при обнаружении объекта лучом поиска.
    /// Реализация определяется в классе-наследнике.
    /// </summary>
    public abstract void ObjectFound();
}
