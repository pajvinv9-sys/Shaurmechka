using UnityEngine;

/// <summary>
/// Реализует поведение ножа, которым можно нарезать подходящие объекты.
///
/// Крепится на: GameObject ножа.
/// Требует: Rigidbody и Collider с включённым Is Trigger.
///
/// Нарезка выполняется при соприкосновении с объектом, реализующим ISliced,
/// если скорость ножа превышает MinCutSpeed.
/// </summary>
public class Knife : GrabbingItem
{
    /// <summary>
    /// Минимальная скорость ножа, необходимая для нарезки объекта.
    /// </summary>
    [field: SerializeField]
    public float MinCutSpeed { get; private set; } = 1f;

    public override void ObjectDrop()
    {
        return;
    }

    public override void ObjectLost()
    {
        return;
    }

    /// <summary>
    /// Проверяет возможность нарезки объекта при входе в триггер ножа.
    /// </summary>
    /// <param name="other">Коллайдер объекта, вошедшего в триггер.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Получаем интерфейс, определяющий возможность нарезки объекта.
        ISliced ingredient = other.GetComponent<ISliced>();

        if (ingredient == null) return;

        // Нарезка выполняется только при достаточной скорости ножа.
        if (Rigidbody.linearVelocity.magnitude < MinCutSpeed) return;

        ingredient.Slice();
    }
}
