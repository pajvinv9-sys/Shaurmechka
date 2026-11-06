using System;
using UnityEngine;

/// <summary>
/// Зона сдачи заказа. Принимает блюдо, сообщает об этом событием и удаляет его.
/// Саму оценку не делает: этим займётся OrderManager.
///
/// Крепится на: объект с коллайдером (Is Trigger) у клиента.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DeliveryZone : MonoBehaviour
{
    /// <summary>
    /// Вызывается до удаления блюда, чтобы подписчик успел прочитать его состав.
    /// </summary>
    public event Action<BaseIngredient> Delivered;
    public event Action<Ingredient> DeliveredIngredient;

    /// <summary>
    /// Принимает ли зона блюда сейчас (OrderManager выключает её, пока заказа нет).
    /// </summary>
    public bool IsAccepting { get; set; } = true;

    private void Reset()
    {
        // Автоматически делаем коллайдер триггером при добавлении компонента.
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsAccepting)
            return;

        BaseIngredient baseIngredient =
            other.GetComponentInParent<BaseIngredient>();

        Ingredient ingredient =
            other.GetComponentInParent<Ingredient>();

        if (baseIngredient != null && baseIngredient.IsCooked)
        {
            Grab grab = Grab.Instance;

            if (grab != null && grab.Item != null &&
                grab.Item.GetComponentInParent<BaseIngredient>() == baseIngredient)
            {
                grab.StopTake();
            }

            Delivered?.Invoke(baseIngredient);

            baseIngredient.gameObject.SetActive(false);
        }
        else if (ingredient != null)
        {
            Grab grab = Grab.Instance;

            if (grab != null && grab.Item != null &&
                grab.Item.GetComponentInParent<Ingredient>() == ingredient)
            {
                grab.StopTake();
            }

            DeliveredIngredient?.Invoke(ingredient);

            Destroy(ingredient.gameObject);
        }
    }
}
