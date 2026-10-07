using UnityEngine;

/// <summary>
/// Превращает триггер баскетбольного кольца в гриль.
/// Вешается на невидимый объект внутри обода кольца.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HoopGrill : MonoBehaviour
{
    private Grab playerGrab;

    private void Start()
    {
        // Автоматически находим скрипт игрока на сцене
        playerGrab = FindFirstObjectByType<Grab>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Cutlet cutlet = other.GetComponent<Cutlet>();
        Rigidbody rb = other.GetComponent<Rigidbody>();
        GrabbingItem grabbingItem = other.GetComponent<GrabbingItem>();

        // Защита: Если игрок всё ещё держит эту котлету в руках — игнорируем
        if (playerGrab != null && playerGrab.Item == grabbingItem)
        {
            return;
        }

        // Защита: Проверяем, что котлета летит именно ВНИЗ (скорость по Y меньше -0.1)
        if (cutlet != null && rb != null && rb.linearVelocity.y < -0.1f)
        {
            cutlet.Cook();
        }
    }
}