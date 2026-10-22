using UnityEngine;

/// <summary>
/// Обрабатывает приготовление котлет при попадании на гриль.
///
/// На объекте должен быть Collider с включённым Is Trigger.
/// Котлета начинает готовиться при входе в триггер во время падения.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HoopGrill : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, является ли вошедший в триггер объект котлетой.
        Cutlet cutlet = other.GetComponent<Cutlet>();

        // Начинаем приготовление, только если котлета движется вниз.
        if (cutlet != null && cutlet.Rigidbody.linearVelocity.y < -0.1f)
        {
            if (cutlet.IsCooked)
                return;

            cutlet.Cook();
            BasketballScore.Instance.AddScore(transform, cutlet);
        }
    }
}
