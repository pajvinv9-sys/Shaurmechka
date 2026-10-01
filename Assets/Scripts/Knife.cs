using UnityEngine;

/// <summary>
/// Скрипт ножа для нарезки ингредиентов.
/// 
/// Вешается на: GameObject ножа или лезвия.
/// Требует: InteractiveObject (для поднятия в руки), Collider (Is Trigger = true), Rigidbody.
/// 
/// Основная задача:
/// - Отслеживает соприкосновение с объектом, имеющим маркер Ingredient (без тегов).
/// - Измеряет текущую скорость движения ножа.
/// - Если скорость ножа достаточна, вызывает метод Slice() у ингредиента.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class Knife : MonoBehaviour
{
    /// <summary>
    /// Минимальная скорость движения ножа для совершения реза.
    /// Если скорость ниже этого значения, нарезка не происходит.
    /// </summary>
    [field: SerializeField]
    public float MinCutSpeed { get; private set; } = 0.5f;

    /// <summary>
    /// Кулдаун между последовательными резами в секундах.
    /// </summary>
    [field: SerializeField]
    public float CutCooldown { get; private set; } = 0.15f;

    /// <summary>
    /// Ссылка на Rigidbody ножа для считывания физической скорости.
    /// </summary>
    public Rigidbody Rb { get; private set; }

    /// <summary>
    /// Позиция ножа в предыдущем кадре для ручного расчета скорости.
    /// </summary>
    private Vector3 lastPosition;

    /// <summary>
    /// Рассчитанная скорость ножа в текущем кадре.
    /// </summary>
    private float currentSpeed;

    /// <summary>
    /// Время последнего успешного реза.
    /// </summary>
    private float lastCutTime;

    private void Awake()
    {
        Rb = GetComponentInParent<Rigidbody>();
        lastPosition = transform.position;
    }

    private void Update()
    {
        CalculateSpeed();
    }

    /// <summary>
    /// Вычисляет текущую скорость движения ножа.
    /// Использует физический linearVelocity (Unity 6) либо смещение позиции.
    /// </summary>
    private void CalculateSpeed()
    {
        if (Rb != null)
        {
            currentSpeed = Rb.linearVelocity.magnitude;
        }
        else
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime > 0f)
            {
                currentSpeed = (transform.position - lastPosition).magnitude / deltaTime;
            }
        }

        lastPosition = transform.position;
    }

    /// <summary>
    /// Обработка входа триггера лезвия в коллайдер ингредиента.
    /// Полностью без тегов: поиск скрипта-маркера Ingredient.
    /// </summary>
    /// <param name="other">Коллайдер коснувшегося объекта.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем кулдаун
        if (Time.time - lastCutTime < CutCooldown)
            return;

        // Проверяем скорость ножа по ТЗ
        if (currentSpeed < MinCutSpeed)
            return;

        // Ищем скрипт-маркер Ingredient на объекте или его родителях
        Ingredient ingredient = other.GetComponentInParent<Ingredient>();

        if (ingredient != null)
        {
            lastCutTime = Time.time;

            // Вызываем нарезку с направлением реза вдоль плоскости ножа
            Vector3 cutDirection = transform.right;
            ingredient.Slice(cutDirection);
        }
    }
}