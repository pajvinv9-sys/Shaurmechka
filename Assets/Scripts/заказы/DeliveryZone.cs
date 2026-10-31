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
    public event Action<Dish> DishDelivered;

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

        Dish dish = other.GetComponentInParent<Dish>();

        if (dish == null || dish.IsDelivered)
            return;

        dish.MarkDelivered();

        // Если блюдо в руках, сначала отпускаем его, иначе Grab останется с мёртвой ссылкой.
        Grab grab = Grab.Instance;
        if (grab != null && grab.Item != null && grab.Item.GetComponentInParent<Dish>() == dish)
            grab.StopTake();

        DishDelivered?.Invoke(dish);

        Destroy(dish.gameObject);
    }
}