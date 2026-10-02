using UnityEngine;

/// <summary>
/// Бесконечный контейнер, который создаёт предметы для захвата.
///
/// Крепится на GameObject контейнера.
///
/// Требует:
/// - InteractiveObject на этом же GameObject.
///
/// Настройка:
/// - В поле ContentPrefab указывается Prefab предмета.
/// - У Prefab должен быть GrabbingItem.
/// </summary>
public class InfiniteBox : InteractiveObject
{
    /// <summary>
    /// Prefab предмета, который будет создаваться контейнером.
    /// </summary>
    [field: SerializeField]
    public GrabbingItem ContentPrefab { get; private set; }

}