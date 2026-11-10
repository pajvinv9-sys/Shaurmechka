using UnityEngine;

/// <summary>
/// Обрабатывает приготовление котлет при касании области гриля.
///
/// На объекте должен быть Collider с включённым Is Trigger.
/// Котлета начинает готовиться при входе в триггер.
/// </summary>
[RequireComponent(typeof(Collider))]
public class GrillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, является ли вошедший объект котлетой.
        Cutlet cutlet = other.GetComponent<Cutlet>();

        if (cutlet == null)
            return;

        // Не готовим котлету повторно.
        if (cutlet.IsCooked)
            return;

        // Начинаем приготовление.
        cutlet.Cook();

        // Начисляем очки.
        BasketballScore.Instance.AddScore(transform, cutlet);
    }
}