using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Физическая конвейерная лента для транспортировки интерактивных объектов.
/// 
/// Вешается на: GameObject конвейера.
/// Требует: Collider (Is Trigger = true) в качестве зоны ленты.
/// 
/// Особенности:
/// - Двигает предметы через rb.linearVelocity (мягкая физика без жестких рывков MovePosition).
/// - Двигает исключительно объекты с маркером InteractiveObject.
/// - Безопасно удаляет уничтоженные объекты (нарезка на ленте) без лишних буферов.
/// - Эффективная анимация текстуры через MaterialPropertyBlock.
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

    /// <summary>
    /// Список физических тел предметов на ленте.
    /// Проход с конца позволяет безопасно удалять элементы на месте без дополнительных буферов.
    /// </summary>
    private readonly List<Rigidbody> itemsOnBelt = new List<Rigidbody>();

    /// <summary>
    /// Блок свойств материала для анимации UV без утечек памяти.
    /// </summary>
    private MaterialPropertyBlock propertyBlock;

    private static readonly int MainTexST = Shader.PropertyToID("_MainTex_ST");
    private static readonly int BaseMapST = Shader.PropertyToID("_BaseMap_ST");

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
            targetTexturePropertyId = BeltRenderer.sharedMaterial != null && BeltRenderer.sharedMaterial.HasProperty(BaseMapST)
                ? BaseMapST
                : MainTexST;
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
        InteractiveObject interactiveObj = other.GetComponent<InteractiveObject>();
        if (interactiveObj == null)
            return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && !rb.isKinematic && !itemsOnBelt.Contains(rb))
        {
            itemsOnBelt.Add(rb);

            // При приземлении гасим вертикальный подскок
            Vector3 currentVel = rb.linearVelocity;
            currentVel.y = 0f;
            rb.linearVelocity = currentVel;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            itemsOnBelt.Remove(rb);
        }
    }

    /// <summary>
    /// Мягкое физическое перемещение предметов через linearVelocity с удалением на месте.
    /// </summary>
    private void MoveItems()
    {
        if (itemsOnBelt.Count == 0)
            return;

        Vector3 targetBeltVelocity = transform.TransformDirection(Direction.normalized) * Speed;

        // Идем с конца: позволяет удалять прямо перед continue без промежуточных буферов
        for (int i = itemsOnBelt.Count - 1; i >= 0; i--)
        {
            Rigidbody rb = itemsOnBelt[i];

            // Если объект был уничтожен прямо на ленте (разрезан ножом)
            if (rb == null)
            {
                itemsOnBelt.RemoveAt(i);
                continue;
            }
            rb.angularVelocity = Vector3.zero;
            
            Vector3 velocity = rb.linearVelocity;
            velocity.x = targetBeltVelocity.x;
            velocity.z = targetBeltVelocity.z;            


            // Если лента наклонная, учитываем и вертикальный уклон targetBeltVelocity.y
            if (Mathf.Abs(targetBeltVelocity.y) > 0.01f)
            {
                velocity.y = targetBeltVelocity.y;
            }

            rb.linearVelocity = velocity;
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
        propertyBlock.SetVector(targetTexturePropertyId, new Vector4(1f, 1f, 0f, textureOffset));
        BeltRenderer.SetPropertyBlock(propertyBlock);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        float arrowLength = 1.5f;
        float arrowHeadSize = 0.35f;

        Vector3 startPoint = transform.position;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            startPoint = col.bounds.center + transform.up * (col.bounds.extents.y + 0.05f);
        }

        Vector3 worldDirection = transform.TransformDirection(Direction.normalized);
        Vector3 endPoint = startPoint + worldDirection * arrowLength;

        Gizmos.DrawLine(startPoint, endPoint);

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