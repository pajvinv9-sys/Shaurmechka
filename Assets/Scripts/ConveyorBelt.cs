using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Физическая конвейерная лента для транспортировки интерактивных объектов.
/// 
/// Вешается на: GameObject конвейера.
/// Требует: Collider (Is Trigger = true) в качестве зоны ленты.
/// 
/// Особенности:
/// - Работает на наклонных конвейерах (корректная проекция на плоскость ленты).
/// - Двигает исключительно объекты с маркером InteractiveObject.
/// - Высокая производительность: HashSet для O(1) и MaterialPropertyBlock без утечек памяти.
/// - Мгновенное гашение отскока при падении.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class ConveyorBelt : MonoBehaviour
{
    [Header("Параметры движения")]
    /// <summary>
    /// Скорость перемещения предметов по конвейеру (м/с).
    /// </summary>
    [field: SerializeField]
    public float Speed { get; private set; } = 2.0f;

    /// <summary>
    /// Локальное направление движения конвейера.
    /// </summary>
    [field: SerializeField]
    public Vector3 Direction { get; private set; } = Vector3.forward;

    /// <summary>
    /// Насколько быстро скорость предметов на ленте стремится к Speed
    /// </summary>
    [field: SerializeField]
    public float BeltAcceleration { get; private set; } = 15.0f;

    /// <summary>
    /// Насколько быстро предметы скидываются с ленты
    /// </summary>
    [field: SerializeField]
    public float BeltAccelerationEnd { get; private set; } = 0.2f;

    [Header("Визуализация ленты")]
    /// <summary>
    /// Меш-рендер полотна для смещения текстуры бегущей дорожки.
    /// </summary>
    [field: SerializeField]
    public Renderer BeltRenderer { get; private set; }

    /// <summary>
    /// Скорость анимации текстуры полотна.
    /// </summary>
    [field: SerializeField]
    public float TextureScrollSpeed { get; private set; } = 1.0f;



    /// <summary>
    /// Множество физических тел интерактивных объектов на ленте.
    /// </summary>
    private readonly HashSet<Rigidbody> itemsOnBelt = new();

    /// <summary>
    /// Блок свойств материала для производительного смещения UV без создания копий материалов.
    /// </summary>
    private MaterialPropertyBlock propertyBlock;

    private float textureOffset;
    private int targetTexturePropertyId;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        if (BeltRenderer == null)
        {
            BeltRenderer = GetComponent<Renderer>();
        }

        if (BeltRenderer != null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
    }

    private void Update()
    {
        AnimateBeltTexture();
    }

    private void FixedUpdate()
    {
        MoveItems();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<InteractiveObject>() == null)
            return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && !rb.isKinematic)
        {
            if (itemsOnBelt.Add(rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            rb.linearVelocity *= BeltAccelerationEnd;
            itemsOnBelt.Remove(rb);
        }
    }

    /// <summary>
    /// Перемещение объектов с учетом наклона поверхности ленты.
    /// </summary>

    private void MoveItems()
    {
        if (itemsOnBelt.Count == 0)
            return;

        Vector3 beltDirection = transform.TransformDirection(Direction.normalized);

        beltDirection = Vector3.ProjectOnPlane(
            beltDirection,
            transform.up
        );

        if (beltDirection.sqrMagnitude < 0.0001f)
            return;

        beltDirection.Normalize();

        foreach (Rigidbody rb in itemsOnBelt)
        {
            if (rb == null || rb.isKinematic)
                continue;

            float alongSpeed = Vector3.Dot(
                rb.linearVelocity,
                beltDirection
            );

            float speedError = Speed - alongSpeed;

            float maxDeltaSpeed =
                BeltAcceleration * Time.fixedDeltaTime;

            float deltaSpeed = Mathf.Clamp(
                speedError,
                -maxDeltaSpeed,
                maxDeltaSpeed
            );

            rb.AddForce(
                beltDirection * deltaSpeed,
                ForceMode.VelocityChange
            );
        }
    }

    /// <summary>
    /// Оптимизированная анимация текстуры через MaterialPropertyBlock.
    /// </summary>
    private void AnimateBeltTexture()
    {
        if (BeltRenderer == null || propertyBlock == null)
            return;

        textureOffset += Speed * TextureScrollSpeed * Time.deltaTime;

        BeltRenderer.GetPropertyBlock(propertyBlock);
        // Vector4(Tiling.x, Tiling.y, Offset.x, Offset.y)
        propertyBlock.SetVector(targetTexturePropertyId, new Vector4(1f, 1f, 0f, textureOffset));
        BeltRenderer.SetPropertyBlock(propertyBlock);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Color gizmoColor = Color.cyan;
        float arrowLength = 1.5f;
        float arrowHeadSize = 0.35f;

        Gizmos.color = gizmoColor;

        Vector3 startPoint = transform.position;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            startPoint = col.bounds.center + transform.up * (col.bounds.extents.y + 0.05f);
        }

        Vector3 worldDirection = transform.TransformDirection(Direction.normalized);
        Vector3 endPoint = startPoint + worldDirection * arrowLength;

        // Основной луч направления
        Gizmos.DrawLine(startPoint, endPoint);

        // Расчет наконечника с учетом наклона плоскости ленты
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