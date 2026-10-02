using UnityEngine;

/// <summary>
/// Скрипт ножа для нарезки ингредиентов.
/// 
/// Вешается на: GameObject ножа.
/// Требует: InteractiveObject, Rigidbody, Collider (Is Trigger = true).
/// 
/// Основная задача:
/// - Отслеживает соприкосновение с объектом, имеющим маркер Ingredient.
/// - Проверяет скорость ножа через Rigidbody.
/// - Если скорость выше MinCutSpeed, вызывает нарезку.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Knife : MonoBehaviour
{
    /// <summary>
    /// Минимальная скорость движения ножа для совершения реза.
    /// </summary>
    [field: SerializeField]
    public float MinCutSpeed { get; private set; } = 0.5f;

    /// <summary>
    /// Ссылка на Rigidbody ножа для получения скорости.
    /// </summary>
    public Rigidbody Rb { get; private set; }

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Метод соприкосновения с ингредиентом.
    /// </summary>
    /// <param name="other">Коллайдер коснувшегося объекта.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (Rb.linearVelocity.magnitude < MinCutSpeed)
            return;

        CanBeSliced ingredient = other.GetComponent<CanBeSliced>();

        if (ingredient != null)
        {
            ingredient.Slice(transform.right);
        }
    }
}