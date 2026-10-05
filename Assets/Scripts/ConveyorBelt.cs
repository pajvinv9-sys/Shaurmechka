using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Физическая конвейерная лента с эффектом мгновенного прилипания предметов (Sticky Belt).
/// 
/// Вешается на: GameObject конвейера.
/// Требует: Collider (Is Trigger = true) в качестве зоны ленты.
/// 
/// Основная задача:
/// - При попадании предмета мгновенно гасит отскок и вращение (эффект прилипания).
/// - Прижимает предметы к полотну во время движения, предотвращая подпрыгивание.
/// - Плавно перемещает все тела вдоль направления движения ленты.
/// - Отображает 3D-стрелку направления в окне Scene.
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
    /// Локальное направление движения ленты.
    /// </summary>
    [field: SerializeField]
    public Vector3 Direction { get; private set; } = Vector3.forward;

    [Header("Настройки прилипания (Анти-отскок)")]
    /// <summary>
    /// Мгновенно гасить вертикальную скорость при приземлении на ленту.
    /// </summary>
    [field: SerializeField]
    public bool DampVelocityOnLand { get; private set; } = true;

    /// <summary>
    /// Сила постоянного легкого прижима предметов к ленте во время движения.
    /// Предотвращает случайные подскоки при соударении с другими овощами.
    /// </summary>
    [field: SerializeField]
    public float StickDownForce { get; private set; } = 9.81f;

    [Header("Визуализация ленты (Необязательно)")]
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

    [Header("Настройки Gizmos (Scene View)")]
    /// <summary>
    /// Показывать ли стрелку направления в окне сцены.
    /// </summary>
    [field: SerializeField]
    public bool ShowGizmos { get; private set; } = true;

    /// <summary>
    /// Цвет стрелки направления в редакторе.
    /// </summary>
    [field: SerializeField]
    public Color GizmoColor { get; private set; } = Color.cyan;

    /// <summary>
    /// Длина стрелки направления в окне Scene.
    /// </summary>
    [field: SerializeField]
    public float ArrowLength { get; private set; } = 1.5f;

    /// <summary>
    /// Список тел, находящихся в данный момент на ленте.
    /// </summary>
    private List<Rigidbody> itemsOnBelt = new List<Rigidbody>();

    /// <summary>
    /// Текущее смещение текстуры.
    /// </summary>
    private float textureOffset;

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
    }

    private void Update()
    {
        AnimateBeltTexture();
    }

    private void FixedUpdate()
    {
        MoveItems();
    }

    /// <summary>
    /// При попадании предмета на ленту моментально гасим отскок и вращение.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;

        if (rb != null && !rb.isKinematic && !itemsOnBelt.Contains(rb))
        {
            itemsOnBelt.Add(rb);

            if (DampVelocityOnLand)
            {
                // Полностью гасим вертикальный отскок и кувыркание
                Vector3 currentVel = rb.linearVelocity;
                currentVel.y = Mathf.Min(0f, currentVel.y * 0.1f); // Убираем положительную скорость вверх
                currentVel.x *= 0.5f;
                currentVel.z *= 0.5f;
                rb.linearVelocity = currentVel;

                // Останавливаем хаотичное вращение от падения
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    /// <summary>
    /// При выходе предмета с конвейера или взятии игроком в руки.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;

        if (rb != null && itemsOnBelt.Contains(rb))
        {
            itemsOnBelt.Remove(rb);
        }
    }

    /// <summary>
    /// Перемещение и мягкий прижим предметов к поверхности ленты.
    /// </summary>
    private void MoveItems()
    {
        Vector3 worldDirection = transform.TransformDirection(Direction.normalized);
        Vector3 movementStep = worldDirection * (Speed * Time.fixedDeltaTime);

        for (int i = itemsOnBelt.Count - 1; i >= 0; i--)
        {
            Rigidbody rb = itemsOnBelt[i];

            // Если предмет был разрезан/уничтожен прямо на ленте
            if (rb == null)
            {
                itemsOnBelt.RemoveAt(i);
                continue;
            }

            // Мягко прижимаем предмет к поверхности конвейера
            if (StickDownForce > 0f)
            {
                rb.AddForce(Vector3.down * StickDownForce, ForceMode.Acceleration);
            }

            // Плавное физическое перемещение вперед
            rb.MovePosition(rb.position + movementStep);
        }
    }

    /// <summary>
    /// Анимация текстуры полотна конвейера.
    /// </summary>
    private void AnimateBeltTexture()
    {
        if (BeltRenderer == null || BeltRenderer.material == null)
            return;

        textureOffset += Speed * TextureScrollSpeed * Time.deltaTime * 0.5f;
        BeltRenderer.material.mainTextureOffset = new Vector2(0f, textureOffset);
    }

    /// <summary>
    /// Отрисовка стрелки направления и начальной точки в окне Scene.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!ShowGizmos)
            return;

        Gizmos.color = GizmoColor;

        Vector3 startPoint = transform.position;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            startPoint = col.bounds.center + Vector3.up * (col.bounds.extents.y + 0.05f);
        }

        Vector3 worldDirection = transform.TransformDirection(Direction.normalized);
        Vector3 endPoint = startPoint + worldDirection * ArrowLength;

        Gizmos.DrawLine(startPoint, endPoint);

        float arrowHeadSize = 0.35f;
        Vector3 right = Vector3.Cross(worldDirection, Vector3.up).normalized;
        if (right == Vector3.zero)
            right = Vector3.Cross(worldDirection, Vector3.right).normalized;

        Vector3 arrowSideA = endPoint - worldDirection * arrowHeadSize + right * (arrowHeadSize * 0.5f);
        Vector3 arrowSideB = endPoint - worldDirection * arrowHeadSize - right * (arrowHeadSize * 0.5f);

        Gizmos.DrawLine(endPoint, arrowSideA);
        Gizmos.DrawLine(endPoint, arrowSideB);
        Gizmos.DrawSphere(startPoint, 0.06f);
    }
}