using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Управляет захватом, перемещением и отпусканием интерактивных объектов.
///
/// Крепится на: GameObject игрока.
/// Требует: LaunchBeam и Rigidbody у захватываемого объекта.
/// Использует: Input Action для отпускания объекта.
///
/// Перемещает захваченный объект к точке перед игроком с помощью пружинной силы
/// и демпфирования, плавно поворачивает его в сторону игрока и позволяет
/// отпустить объект с импульсом в направлении взгляда.
/// </summary>
public class Grab : MonoBehaviour
{
    /// <summary>
    /// Расстояние от игрока до точки удержания объекта.
    /// </summary>
    [field: SerializeField]
    public float Distance { get; private set; }

    /// <summary>
    /// Текущий захваченный объект. Null, если захват не выполняется.
    /// </summary>
    public GrabbingItem Item { get; private set; }

    /// <summary>
    /// Компонент, отвечающий за поиск интерактивных объектов.
    /// </summary>
    public LaunchBeam LaunchBeam { get; private set; }

    /// <summary>
    /// Input Action для отпускания захваченного объекта.
    /// </summary>
    [field: SerializeField]
    public InputAction DropKey { get; private set; }

    /// <summary>
    /// Коэффициент пружинной силы, возвращающей объект к точке удержания.
    /// </summary>
    [field: SerializeField]
    public float spring { get; private set; } = 20f;

    /// <summary>
    /// Коэффициент демпфирования, уменьшающего колебания объекта.
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

    // Текущая целевая позиция захваченного объекта.
    private Vector3 grabPoint;

    // Ссылка на корутину, управляющую удержанием объекта.
    private Coroutine takeCoroutine;

    // Позволяет другим компонентам получить доступ к единственному экземпляру Grab.
    public static Grab Instance;

    private void Awake()
    {
        // Не допускаем существования нескольких экземпляров Grab.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Получаем компонент поиска с того же GameObject, если он не назначен.
        if (LaunchBeam == null)
            LaunchBeam = GetComponent<LaunchBeam>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnEnable()
    {
        LaunchBeam.BeamKey.canceled += OnEndTake;

        // Подписываемся на отпускание объекта и активируем соответствующее действие.
        DropKey.started += Drop;
        DropKey.Enable();
    }

    private void OnDisable()
    {
        LaunchBeam.BeamKey.canceled -= OnEndTake;

        DropKey.started -= Drop;
        DropKey.Disable();

        // Завершаем захват, если компонент отключается во время взаимодействия.
        StopTake();
    }

    /// <summary>
    /// Начинает удержание переданного объекта.
    /// </summary>
    /// <param name="interactiveObject">Объект, который необходимо захватить.</param>
    public void StartTake(GrabbingItem interactiveObject)
    {
        Item = interactiveObject;
        takeCoroutine = StartCoroutine(Take());
    }

    /// <summary>
    /// Удерживает объект на заданном расстоянии перед игроком
    /// и плавно поворачивает его в сторону игрока.
    /// </summary>
    private IEnumerator Take()
    {
        while (Item != null)
        {
            // Пересчитываем точку удержания с учётом текущего положения игрока.
            grabPoint =
                transform.position +
                transform.forward * Distance;

            Vector3 positionError =
                grabPoint - Item.transform.position;

            // Пружина тянет объект к целевой точке, демпфирование гасит его скорость.
            Vector3 force =
                positionError * spring -
                Item.Rigidbody.linearVelocity * damping;

            Item.Rigidbody.AddForce(force, ForceMode.Force);

            // Определяем направление от объекта к игроку без учёта высоты.
            Vector3 directionToPlayer =
                transform.position - Item.transform.position;

            directionToPlayer.y = 0f;

            if (directionToPlayer.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(directionToPlayer);

                // Плавно поворачиваем объект к игроку с заданной угловой скоростью.
                Quaternion newRotation =
                    Quaternion.RotateTowards(
                        Item.Rigidbody.rotation,
                        targetRotation,
                        RotationSpeed * Time.deltaTime
                    );

                Item.Rigidbody.MoveRotation(newRotation);
            }

            yield return null;
        }

        // Очищаем ссылку после естественного завершения корутины.
        takeCoroutine = null;
    }

    /// <summary>
    /// Завершает захват при отмене действия поиска.
    /// </summary>
    private void OnEndTake(InputAction.CallbackContext context)
    {
        StopTake();
    }

    /// <summary>
    /// Прекращает удержание объекта и сбрасывает состояние захвата.
    /// </summary>
    public void StopTake()
    {
        if (takeCoroutine != null)
        {
            StopCoroutine(takeCoroutine);
            takeCoroutine = null;
        }

        if (Item != null)
            Item.ObjectLost();

        Item = null;
    }

    /// <summary>
    /// Отпускает объект и придаёт ему импульс в направлении взгляда игрока.
    /// </summary>
    /// <param name="context">Контекст вызова Input Action.</param>
    private void Drop(InputAction.CallbackContext context)
    {
        if (Item == null)
            return;

        Item.Rigidbody.AddForce(
            transform.forward * DropForce,
            ForceMode.Impulse
        );

        Item.ObjectDrop();

        // Завершаем захват после передачи объекту импульса.
        StopTake();
    }
}
