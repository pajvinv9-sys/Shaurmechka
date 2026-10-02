using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Управляет захватом и перемещением интерактивного объекта перед игроком.
/// 
/// Крепится на: GameObject игрока.
/// Требует: LaunchBeam, GrabbingItem у захватываемого объекта, Rigidbody.
/// Использует: Input Action для отпускания объекта.
/// 
/// Основная логика:
/// - Получает интерактивный объект через события LaunchBeam.
/// - Перемещает объект к точке перед игроком с помощью пружинной силы.
/// - Использует демпфирование для уменьшения колебаний объекта.
/// - Поворачивает захваченный объект в сторону игрока.
/// - Позволяет отпустить объект по нажатию DropKey.
/// - При отпускании добавляет объекту импульс в направлении взгляда игрока.
/// </summary>
public class Grab : MonoBehaviour
{
    /// <summary>
    /// Расстояние от игрока до точки, в которую перемещается захваченный объект.
    /// </summary>
    [field: SerializeField]
    public float Distance { get; private set; }

    /// <summary>
    /// Текущий захваченный объект.
    /// Если объект не захвачен, значение равно null.
    /// </summary>
    public GrabbingItem Item { get; private set; }

    /// <summary>
    /// Компонент, отвечающий за поиск интерактивных объектов перед игроком.
    /// </summary>
    public LaunchBeam Lb { get; private set; }

    /// <summary>
    /// Input Action для отпускания захваченного объекта.
    /// </summary>
    [field: SerializeField]
    public InputAction DropKey { get; private set; }

    /// <summary>
    /// Коэффициент пружинной силы, которая притягивает объект к точке захвата.
    /// Чем выше значение, тем быстрее объект стремится к точке захвата.
    /// </summary>
    [field: SerializeField]
    public float spring { get; private set; } = 20f;

    /// <summary>
    /// Коэффициент демпфирования скорости захваченного объекта.
    /// Используется для уменьшения колебаний при перемещении.
    /// </summary>
    [field: SerializeField]
    public float damping { get; private set; } = 5f;

    /// <summary>
    /// Сила импульса, прикладываемого к объекту при отпускании.
    /// </summary>
    [field: SerializeField]
    public float DropForce { get; private set; } = 5f;

    /// <summary>
    /// Скорость поворота захваченного объекта в сторону игрока.
    /// </summary>
    [field: SerializeField]
    public float RotationSpeed { get; private set; } = 5f;

    /// <summary>
    /// Текущая точка, в которую должен перемещаться захваченный объект.
    /// </summary>
    private Vector3 grabPoint;

    /// <summary>
    /// Coroutine, отвечающий за удержание объекта перед игроком.
    /// </summary>
    private Coroutine takeCoroutine;

    private void Awake()
    {
        // Если LaunchBeam не назначен, пытаемся получить его с этого же GameObject.
        if (Lb == null)
            Lb = GetComponent<LaunchBeam>();
    }

    private void OnEnable()
    {
        // Подписываемся на события обнаружения и потери интерактивного объекта.
        Lb.ItemFound += OnStartTake;
        Lb.ItemLost += OnEndTake;

        // Включаем Input Action и подписываемся на событие отпускания объекта.
        DropKey.started += Drop;
        DropKey.Enable();
    }

    private void OnDisable()
    {
        // Отписываемся от событий LaunchBeam.
        Lb.ItemFound -= OnStartTake;
        Lb.ItemLost -= OnEndTake;

        // Отписываемся от Input Action и отключаем его.
        DropKey.started -= Drop;
        DropKey.Disable();

        // Останавливаем захват при отключении компонента.
        StopTake();
    }

    /// <summary>
    /// Начинает захват обнаруженного интерактивного объекта.
    /// </summary>
    /// <param name="interactiveObject">Обнаруженный интерактивный объект.</param>
    public void OnStartTake(InteractiveObject interactiveObject)
    {
        // Получаем компонент, позволяющий захватывать объект.
        Item = interactiveObject.GetComponent<GrabbingItem>();

        // Если объект не поддерживает захват, ничего не делаем.
        if (Item == null)
            return;

        // Запускаем Coroutine для постоянного удержания объекта перед игроком.
        takeCoroutine = StartCoroutine(Take());
    }

    /// <summary>
    /// Удерживает захваченный объект в точке перед игроком
    /// и постепенно поворачивает его в сторону игрока.
    /// </summary>
    private IEnumerator Take()
    {
        while (Item != null)
        {
            // Рассчитываем точку захвата относительно позиции и направления игрока.
            grabPoint =
                transform.position +
                transform.forward * Distance;

            // Определяем расстояние от объекта до точки захвата.
            Vector3 positionError =
                grabPoint - Item.transform.position;

            // Рассчитываем силу, которая возвращает объект к точке захвата.
            // Первая часть работает как пружина, вторая уменьшает скорость объекта.
            Vector3 force =
                positionError * spring -
                Item.Rb.linearVelocity * damping;

            // Прикладываем рассчитанную силу к Rigidbody объекта.
            Item.Rb.AddForce(force, ForceMode.Force);

            // Рассчитываем направление от объекта к игроку.
            Vector3 directionToPlayer =
                transform.position - Item.transform.position;

            // Игнорируем вертикальное направление при расчёте поворота.
            directionToPlayer.y = 0f;

            // Проверяем, что направление достаточно большое для расчёта поворота.
            if (directionToPlayer.sqrMagnitude > 0.001f)
            {
                // Создаём поворот, направляющий объект в сторону игрока.
                Quaternion targetRotation =
                    Quaternion.LookRotation(directionToPlayer);

                // Плавно приближаем текущий поворот объекта к целевому.
                Quaternion newRotation =
                    Quaternion.RotateTowards(
                        Item.Rb.rotation,
                        targetRotation,
                        RotationSpeed * Time.deltaTime
                    );

                // Применяем новый поворот через Rigidbody.
                Item.Rb.MoveRotation(newRotation);
            }

            // Ждём следующий кадр перед повторным расчётом.
            yield return null;
        }

        // Coroutine завершена, поэтому очищаем её ссылку.
        takeCoroutine = null;
    }

    /// <summary>
    /// Обрабатывает потерю текущего интерактивного объекта.
    /// </summary>
    private void OnEndTake()
    {
        StopTake();
    }

    /// <summary>
    /// Прекращает захват текущего объекта и очищает состояние захвата.
    /// </summary>
    private void StopTake()
    {
        // Если Coroutine захвата запущена, останавливаем её.
        if (takeCoroutine != null)
        {
            StopCoroutine(takeCoroutine);
            takeCoroutine = null;
        }

        // Сбрасываем ссылку на захваченный объект.
        Item = null;
    }

    /// <summary>
    /// Отпускает захваченный объект и придаёт ему импульс вперёд.
    /// </summary>
    /// <param name="context">Контекст вызова Input Action.</param>
    private void Drop(InputAction.CallbackContext context)
    {
        // Если объект не захвачен, ничего не делаем.
        if (Item == null)
            return;

        // Придаём объекту импульс в направлении взгляда игрока.
        Item.Rb.AddForce(
            transform.forward * DropForce,
            ForceMode.Impulse
        );

        // Завершаем захват после применения импульса.
        StopTake();
    }
}