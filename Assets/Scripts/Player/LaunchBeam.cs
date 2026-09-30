using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Управляет лучом поиска интерактивных объектов перед игроком.
/// 
/// Крепится на: GameObject игрока.
/// Использует: Input Action для активации луча.
/// 
/// Основная логика:
/// - Активирует поиск при нажатии BeamKey.
/// - Выпускает Raycast в направлении взгляда объекта.
/// - Ищет InteractiveObject на объекте или его родительском GameObject.
/// - Ограничивает дальность поиска параметром MaxLengthOfBeam.
/// - Сообщает о найденном объекте через событие ItemFound.
/// - Сообщает о прекращении поиска или потере объекта через ItemLost.
/// </summary>
public class LaunchBeam : MonoBehaviour
{
    /// <summary>
    /// Input Action для активации луча поиска.
    /// </summary>
    [field: SerializeField]
    public InputAction BeamKey { get; private set; }

    /// <summary>
    /// Текущий интерактивный объект, найденный лучом.
    /// Если объект не найден, значение равно null.
    /// </summary>
    public InteractiveObject Item { get; private set; }

    /// <summary>
    /// Максимальная дальность действия луча поиска.
    /// </summary>
    [field: SerializeField]
    public float MaxLengthOfBeam { get; private set; }

    /// <summary>
    /// Вызывается, когда луч обнаруживает интерактивный объект.
    /// Передаёт найденный объект в качестве аргумента.
    /// </summary>
    public event Action<InteractiveObject> ItemFound;

    /// <summary>
    /// Вызывается, когда текущий интерактивный объект перестаёт быть выбранным.
    /// </summary>
    public event Action ItemLost;

    /// <summary>
    /// Луч, используемый для поиска интерактивных объектов.
    /// </summary>
    private Ray ray;

    /// <summary>
    /// Информация о пересечении луча с объектом.
    /// </summary>
    private RaycastHit hit;

    /// <summary>
    /// Coroutine, отвечающая за поиск интерактивного объекта.
    /// </summary>
    private Coroutine searchingCoroutine;

    private void OnEnable()
    {
        // Включаем Input Action для получения ввода.
        BeamKey.Enable();

        // Начинаем поиск при нажатии и прекращаем при отпускании клавиши.
        BeamKey.started += OnLaunchBeam;
        BeamKey.canceled += OnStopBeam;
    }

    private void OnDisable()
    {
        // Отписываемся от событий Input Action.
        BeamKey.started -= OnLaunchBeam;
        BeamKey.canceled -= OnStopBeam;

        // Отключаем Input Action.
        BeamKey.Disable();

        // Останавливаем поиск и сбрасываем найденный объект.
        StopSearching();
    }

    /// <summary>
    /// Запускает Coroutine поиска интерактивного объекта.
    /// </summary>
    /// <param name="context">Контекст вызова Input Action.</param>
    private void OnLaunchBeam(InputAction.CallbackContext context)
    {
        // Не запускаем дополнительный поиск, если он уже выполняется.
        if (searchingCoroutine != null)
            return;

        searchingCoroutine = StartCoroutine(Searching());
    }

    /// <summary>
    /// Останавливает поиск интерактивного объекта.
    /// </summary>
    /// <param name="context">Контекст вызова Input Action.</param>
    private void OnStopBeam(InputAction.CallbackContext context)
    {
        StopSearching();
    }

    /// <summary>
    /// Останавливает Coroutine поиска и сбрасывает текущий найденный объект.
    /// </summary>
    private void StopSearching()
    {
        // Если поиск выполняется, останавливаем его.
        if (searchingCoroutine != null)
        {
            StopCoroutine(searchingCoroutine);
            searchingCoroutine = null;
        }

        // Если объект был найден, очищаем ссылку и уведомляем подписчиков.
        if (Item != null)
        {
            Item = null;
            ItemLost?.Invoke();
        }
    }

    /// <summary>
    /// Выполняет поиск интерактивного объекта перед игроком
    /// до тех пор, пока BeamKey удерживается.
    /// </summary>
    private IEnumerator Searching()
    {
        while (BeamKey.IsPressed())
        {
            // Создаём луч из текущей позиции объекта в направлении его взгляда.
            ray = new Ray(transform.position, transform.forward);

            // Проверяем пересечение луча с объектами в пределах максимальной дальности.
            if (Physics.Raycast(ray, out hit, MaxLengthOfBeam))
            {
                // Ищем InteractiveObject на объекте или одном из его родителей.
                InteractiveObject newItem =
                    hit.collider.GetComponentInParent<InteractiveObject>();

                // Если интерактивный объект найден, сохраняем его и уведомляем подписчиков.
                if (newItem != null)
                {
                    Item = newItem;

                    ItemFound?.Invoke(Item);

                    // Объект найден, поэтому дальнейший поиск не требуется.
                    yield break;
                }
            }

            // Ждём следующий кадр и повторяем проверку.
            yield return null;
        }

        // Coroutine завершена без нахождения объекта.
        searchingCoroutine = null;
    }
}