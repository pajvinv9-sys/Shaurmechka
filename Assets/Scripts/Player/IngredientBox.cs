using UnityEngine;

/// <summary>
/// Управляет спавном предметов из коробок при взаимодействии.
/// 
/// Крепится на: GameObject игрока.
/// Требует: LaunchBeam для получения событий, Grab для захвата нового предмета.
/// 
/// Основная логика:
/// - Слушает событие ItemFound от LaunchBeam (как и Grab).
/// - Проверяет, является ли найденный объект маркером IngredientBoxMarker.
/// - Если да, создает копию префаба и немедленно передает ее в Grab.
/// </summary>
public class IngredientBox : MonoBehaviour
{
    public LaunchBeam Lb { get; private set; }

    // Ссылка на существующую логику захвата игрока
    public Grab PlayerGrab { get; private set; }

    private void Awake()
    {
        if (Lb == null)
            Lb = GetComponent<LaunchBeam>();

        if (PlayerGrab == null)
            PlayerGrab = GetComponent<Grab>();
    }

    private void OnEnable()
    {
        // Подписываемся на события обнаружения объекта, точно как в Grab.cs
        Lb.ItemFound += OnBoxFound;
    }

    private void OnDisable()
    {
        // Отписываемся при отключении
        Lb.ItemFound -= OnBoxFound;
    }

    /// <summary>
    /// Срабатывает, когда луч находит любой интерактивный объект.
    /// </summary>
    private void OnBoxFound(InteractiveObject interactiveObject)
    {
        // Пытаемся получить маркер коробки
        IngredientBoxMarker box = interactiveObject.GetComponent<IngredientBoxMarker>();

        // Если это не коробка, или в ней не назначен префаб продукта — игнорируем
        if (box == null || box.ItemPrefab == null)
            return;

        // 1. Создаем новый предмет из префаба коробки (чуть выше ее центра, чтобы он не застрял в коллайдере)
        GrabbingItem spawnedItem = Instantiate(
            box.ItemPrefab,
            box.transform.position + Vector3.up * 0.5f,
            Quaternion.identity
        );

        // 2. Имитируем, что луч нашел именно этот новый предмет, 
        // насильно передавая его в публичный метод OnStartTake системы Grab.
        if (PlayerGrab != null)
        {
            PlayerGrab.OnStartTake(spawnedItem.Io);
        }
    }
}