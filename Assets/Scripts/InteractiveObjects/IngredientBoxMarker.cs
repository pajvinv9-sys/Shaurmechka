using UnityEngine;

/// <summary>
/// Обозначает объект как бесконечную коробку с предметами.
/// Крепится на: GameObject коробки (источника).
/// 
/// Основная логика:
/// - Хранит префаб предмета, который нужно выдавать.
/// - Как и GrabbingItem, автоматически добавляет InteractiveObject при старте.
/// </summary>
public class IngredientBoxMarker : MonoBehaviour
{
    /// <summary>
    /// Префаб предмета. На нем обязательно должен висеть GrabbingItem.
    /// </summary>
    [field: SerializeField]
    public GrabbingItem ItemPrefab { get; private set; }

    public InteractiveObject Io { get; private set; }

    void Start()
    {
        // Автоматически добавляем InteractiveObject, если его нет (по паттерну GrabbingItem)
        Io = GetComponent<InteractiveObject>();

        if (Io == null)
            Io = gameObject.AddComponent<InteractiveObject>();
    }
}