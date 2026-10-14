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
/// - Запускает поиск при нажатии BeamKey.
/// - Выпускает Raycast в направлении взгляда объекта.
/// - Ищет InteractiveObject на объекте или его родителях.
/// - Ограничивает дальность поиска параметром MaxLengthOfBeam.
/// - Передаёт найденный объект в обработчик взаимодействия.
/// - Останавливает поиск при отпускании клавиши или отключении компонента.
/// </summary>
public class LaunchBeam : MonoBehaviour
{
    /// <summary>
    /// Input Action для активации луча поиска.
    /// </summary>
    [field: SerializeField]
    public InputAction BeamKey { get; private set; }

    /// <summary>
    /// Максимальная дальность действия луча поиска.
    /// </summary>
    [field: SerializeField]
    public float MaxLengthOfBeam { get; private set; }

    // Луч и результат его пересечения с объектами сцены.
    private Ray ray;
    private RaycastHit hit;

    // Ссылка на текущую корутину поиска, чтобы не запускать её повторно.
    private Coroutine searchingCoroutine;

    private void OnEnable()
    {
        BeamKey.Enable();

        // Запускаем поиск при нажатии и останавливаем при отпускании клавиши.
        BeamKey.started += OnLaunchBeam;
        BeamKey.canceled += OnStopBeam;
    }

    private void OnDisable()
    {
        BeamKey.started -= OnLaunchBeam;
        BeamKey.canceled -= OnStopBeam;
        BeamKey.Disable();

        // При отключении компонента прекращаем поиск и текущее взаимодействие.
        StopSearching();
    }

    public void StartSearching()
    {
        // Не допускаем одновременного запуска нескольких корутин поиска.
        if (searchingCoroutine != null)
            return;

        searchingCoroutine = StartCoroutine(Searching());
    }

    /// <summary>
    /// Ищет интерактивный объект перед игроком, пока удерживается BeamKey.
    /// </summary>
    private IEnumerator Searching()
    {
        while (BeamKey.IsPressed())
        {
            // Каждый кадр проверяем пространство перед игроком с учётом его текущего направления.
            ray = new Ray(transform.position, transform.forward);

            if (Physics.Raycast(ray, out hit, MaxLengthOfBeam))
            {
                // Ищем компонент на объекте попадания или выше по иерархии.
                InteractiveObject newItem =
                    hit.collider.GetComponentInParent<InteractiveObject>();

                if (newItem != null)
                {
                    // Передаём найденный объект его собственному обработчику обнаружения.
                    newItem.ObjectFound();
                    yield break;
                }
            }

            // Продолжаем поиск на следующем кадре.
            yield return null;
        }

        // Сбрасываем ссылку после естественного завершения поиска.
        searchingCoroutine = null;
    }

    public void StopSearching()
    {
        if (searchingCoroutine != null)
        {
            StopCoroutine(searchingCoroutine);
            searchingCoroutine = null;
        }

        // Если объект удерживается, прекращаем взаимодействие с ним.
        if (Grab.Instance != null && Grab.Instance.Item != null)
        {
            Grab.Instance.StopTake();
        }
    }

    /// <summary>
    /// Обрабатывает отпускание клавиши поиска.
    /// </summary>
    /// <param name="context">Контекст вызова Input Action.</param>
    private void OnStopBeam(InputAction.CallbackContext context)
    {
        StopSearching();
    }

    /// <summary>
    /// Обрабатывает нажатие клавиши и запускает поиск.
    /// </summary>
    /// <param name="context">Контекст вызова Input Action.</param>
    private void OnLaunchBeam(InputAction.CallbackContext context)
    {
        StartSearching();
    }
}
