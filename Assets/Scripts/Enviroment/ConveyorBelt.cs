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
    [field: SerializeField] bool isActive = true;

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
        GetComponent<Collider>().isTrigger = true;
    }

    private void FixedUpdate()
    {
        if (isActive) Move();
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb =
            other.GetComponent<Rigidbody>();

        if (rb == null)
            return;

        itemsOnBelt.Add(rb);
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb =
            other.GetComponent<Rigidbody>();

        if (rb == null) 
            return;
        
        itemsOnBelt.Remove(rb);
    }

    private void OnDisable()
    {
        // Очищаем список при отключении конвейера.
        itemsOnBelt.Clear();
    }

    public void SwitchActive()
    {
        if (!isActive) On();
        else Off();
    }

    public void On() 
        => isActive = true;

    public void Off()
    {
        isActive = false;

        foreach (var item in itemsOnBelt)
        {
            item.linearVelocity *= 0.03f;
            item.angularVelocity *= 0.03f;
        }
    }

    public void SwitchDirection()
        => direction *= -1;


    private void Move()
    {
        // Пропускаем обновление, если направление движения не задано корректно.
        if (BeltDirection.sqrMagnitude < 0.001f)
            return;

        foreach (var rb in itemsOnBelt)
        {
            if (rb == null || rb.isKinematic)
                continue;
            // Сохраняем скорость перпендикулярно поверхности,
            // чтобы конвейер не подавлял падение и подпрыгивание предмета.
            Vector3 normalVelocity =
                Vector3.Project(rb.linearVelocity, BeltNormal);

            Vector3 tangentVelocity =
                Vector3.ProjectOnPlane(rb.linearVelocity, BeltNormal);
            // Выделяем составляющую скорости вдоль поверхности конвейера.

            // Задаём скорость, к которой должен постепенно разогнаться предмет.
            Vector3 targetVelocity = BeltDirection * speed;

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
                    Vector3.Dot(tangentVelocity, BeltDirection);

                Vector3 lateralVelocity =
                    tangentVelocity - BeltDirection * forwardSpeed;

                lateralVelocity = Vector3.MoveTowards(
                    lateralVelocity,
                    Vector3.zero,
                    lateralCorrection * Time.fixedDeltaTime
                );

                tangentVelocity =
                    BeltDirection * forwardSpeed + lateralVelocity;
            }

            // Компенсируем влияние гравитации вдоль поверхности,
            // чтобы предметы не замедлялись на подъёмах и не ускорялись на спусках.
            if (compensateGravity)
            {
                Vector3 gravityAlongSurface =
                    Vector3.ProjectOnPlane(Physics.gravity, BeltNormal);

                rb.AddForce(
                    -gravityAlongSurface,
                    ForceMode.Acceleration
                );
            }

            Vector3 velocityDifference =
                targetVelocity - tangentVelocity;

            // Сила пропорциональна массе предмета.
            Vector3 force =
                velocityDifference * beltAcceleration * rb.mass;

            rb.AddForce(force, ForceMode.Force);
        }

        // Удаляем ссылки на уничтоженные объекты.
        itemsOnBelt.RemoveWhere(rb => rb == null);
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
