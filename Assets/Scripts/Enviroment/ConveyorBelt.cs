using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Управляет физическим перемещением интерактивных объектов по конвейеру.
///
/// Поддерживает наклонные поверхности и последовательные сегменты
/// с разными направлениями движения.
/// Обрабатывает только объекты с компонентом InteractiveObject.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class ConveyorBelt : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField, Min(0f)]
    private float speed = 2f;

    [SerializeField]
    private Vector3 direction = Vector3.forward;

    [Tooltip("Ускорение движения предмета вдоль ленты.")]
    [SerializeField, Min(0f)]
    private float beltAcceleration = 12f;

    [Header("Удержание на ленте")]
    [Tooltip("Компенсировать гравитацию вдоль поверхности.")]
    [SerializeField]
    private bool compensateGravity = true;

    [Tooltip("Скорость подавления движения поперёк ленты.")]
    [SerializeField, Min(0f)]
    private float lateralCorrection = 3f;

    // Rigidbody объектов, которые в данный момент находятся на конвейере.
    private readonly HashSet<Rigidbody> itemsOnBelt = new();

    // Нормаль поверхности конвейера в мировых координатах.
    private Vector3 BeltNormal =>
        transform.up.normalized;

    // Направление движения в мировых координатах,
    // спроецированное на плоскость поверхности конвейера.
    private Vector3 BeltDirection
    {
        get
        {
            Vector3 worldDirection =
                transform.TransformDirection(direction);

            return Vector3.ProjectOnPlane(
                worldDirection, BeltNormal
            ).normalized;
        }
    }

    private void Awake()
    {
        // Используем триггер для определения объектов, попавших на конвейер.
        GetComponent<Collider>().isTrigger = true;
    }

    private void FixedUpdate()
    {
        Vector3 normal = BeltNormal;
        Vector3 forward = BeltDirection;

        // Пропускаем обновление, если направление движения не задано корректно.
        if (forward.sqrMagnitude < 0.001f)
            return;

        foreach (Rigidbody rb in itemsOnBelt)
        {
            if (rb == null || rb.isKinematic)
                continue;

            Vector3 velocity = rb.linearVelocity;

            // Выделяем составляющую скорости вдоль поверхности конвейера.
            Vector3 tangentVelocity =
                Vector3.ProjectOnPlane(velocity, normal);

            // Задаём скорость, к которой должен постепенно разогнаться предмет.
            Vector3 targetVelocity = forward * speed;

            // Постепенно приближаем скорость предмета к скорости ленты.
            tangentVelocity = Vector3.MoveTowards(
                tangentVelocity,
                targetVelocity,
                beltAcceleration * Time.fixedDeltaTime
            );

            // Уменьшаем боковое скольжение, сохраняя скорость вдоль ленты.
            if (lateralCorrection > 0f)
            {
                float forwardSpeed =
                    Vector3.Dot(tangentVelocity, forward);

                Vector3 lateralVelocity =
                    tangentVelocity - forward * forwardSpeed;

                lateralVelocity = Vector3.MoveTowards(
                    lateralVelocity,
                    Vector3.zero,
                    lateralCorrection * Time.fixedDeltaTime
                );

                tangentVelocity =
                    forward * forwardSpeed + lateralVelocity;
            }

            // Компенсируем влияние гравитации вдоль поверхности,
            // чтобы предметы не замедлялись на подъёмах и не ускорялись на спусках.
            if (compensateGravity)
            {
                Vector3 gravityAlongSurface =
                    Vector3.ProjectOnPlane(Physics.gravity, normal);

                rb.AddForce(
                    -gravityAlongSurface,
                    ForceMode.Acceleration
                );
            }

            // Сохраняем скорость перпендикулярно поверхности,
            // чтобы конвейер не подавлял падение и подпрыгивание предмета.
            Vector3 normalVelocity =
                Vector3.Project(velocity, normal);

            rb.linearVelocity =
                tangentVelocity + normalVelocity;
        }

        // Удаляем ссылки на уничтоженные объекты.
        itemsOnBelt.RemoveWhere(rb => rb == null);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, относится ли объект к системе интерактивных объектов.
        InteractiveObject interactive =
            other.GetComponentInParent<InteractiveObject>();

        if (interactive == null)
            return;

        // Получаем Rigidbody объекта, вошедшего в триггер.
        Rigidbody rb = other.attachedRigidbody;

        if (rb == null)
            rb = interactive.GetComponentInParent<Rigidbody>();

        // Добавляем физический объект в список управляемых конвейером.
        if (rb != null && !rb.isKinematic)
            itemsOnBelt.Add(rb);
    }

    private void OnTriggerExit(Collider other)
    {
        // Определяем Rigidbody объекта, покинувшего триггер.
        Rigidbody rb = other.attachedRigidbody;

        if (rb == null)
        {
            InteractiveObject interactive =
                other.GetComponentInParent<InteractiveObject>();

            if (interactive != null)
                rb = interactive.GetComponentInParent<Rigidbody>();
        }

        // Прекращаем управлять объектом после выхода из триггера.
        if (rb != null)
            itemsOnBelt.Remove(rb);
    }

    private void OnDisable()
    {
        // Очищаем список при отключении конвейера.
        itemsOnBelt.Clear();
    }

    private void OnValidate()
    {
        // Не допускаем отрицательных значений параметров движения.
        speed = Mathf.Max(0f, speed);
        beltAcceleration = Mathf.Max(0f, beltAcceleration);
        lateralCorrection = Mathf.Max(0f, lateralCorrection);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Color gizmoColor = Color.cyan;
        float arrowLength = 1.5f;
        float arrowHeadSize = 0.35f;

        Gizmos.color = gizmoColor;

        // Размещаем начало стрелки немного выше поверхности конвейера.
        Vector3 startPoint = transform.position;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            startPoint = col.bounds.center + transform.up * (col.bounds.extents.y + 0.05f);
        }

        Vector3 worldDirection = transform.TransformDirection(direction.normalized);
        Vector3 endPoint = startPoint + worldDirection * arrowLength;

        // Отображаем направление движения конвейера в редакторе.
        Gizmos.DrawLine(startPoint, endPoint);

        // Рассчитываем боковые линии наконечника с учётом ориентации конвейера.
        Vector3 right = Vector3.Cross(worldDirection, transform.up).normalized;
        if (right == Vector3.zero)
            right = transform.right;

        Vector3 arrowSideA = endPoint - worldDirection * arrowHeadSize + right * (arrowHeadSize * 0.5f);
        Vector3 arrowSideB = endPoint - worldDirection * arrowHeadSize - right * (arrowHeadSize * 0.5f);

        Gizmos.DrawLine(endPoint, arrowSideA);
        Gizmos.DrawLine(endPoint, arrowSideB);
        Gizmos.DrawSphere(startPoint, 0.05f);
    }
#endif
}
