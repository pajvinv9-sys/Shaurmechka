using UnityEngine;

/// <summary>
/// Скрипт-маркер и логика нарезки объекта на дольки или слайсы.
/// 
/// Вешается на: GameObject нарезаемого объекта (овощ, фрукт, ингредиент).
/// Требует: InteractiveObject (для поднятия в руки), Collider, Rigidbody.
/// 
/// Основная задача:
/// - Служит маркером для ножа (Knife.cs).
/// - Если назначен SlicedPiecePrefab — спавнит готовые префабы (дольки помидора, слайсы огурца).
/// - Если префаб НЕ назначен — автоматически делит текущий объект на 3 уменьшенные копии (режим прототипа).
/// - Навешивает маркер InteractiveObject на каждую дольку, чтобы их можно было поднимать в руки.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class CanBeSliced : MonoBehaviour
{
    [Header("Префаб дольки / слайса (Необязательно)")]
    /// <summary>
    /// Префаб готовой дольки или слайса.
    /// Если поле пустое (None), скрипт делит текущий меш на 3 части (для тестов без моделей).
    /// </summary>
    [field: SerializeField]
    public GameObject SlicedPiecePrefab { get; private set; }

    /// <summary>
    /// Количество получаемых долек/слайсов при нарезке.
    /// </summary>
    [field: SerializeField]
    public int SliceCount { get; private set; } = 3;

    [Header("Физика разлета")]
    /// <summary>
    /// Сила расталкивания долек в стороны от линии реза.
    /// </summary>
    [field: SerializeField]
    public float SeparationForce { get; private set; } = 2.0f;

    /// <summary>
    /// Подброс долек вверх для сочного эффекта.
    /// </summary>
    [field: SerializeField]
    public float UpwardBias { get; private set; } = 0.6f;

    /// <summary>
    /// Сила случайного вращения долек при разлете.
    /// </summary>
    [field: SerializeField]
    public float TorqueAmount { get; private set; } = 1.5f;

    [Header("Эффекты")]
    /// <summary>
    /// Звук нарезки овоща.
    /// </summary>
    [field: SerializeField]
    public AudioClip CutSound { get; private set; }

    /// <summary>
    /// Эффект брызг сока / частиц при нарезке.
    /// </summary>
    [field: SerializeField]
    public ParticleSystem JuiceVFX { get; private set; }

    /// <summary>
    /// Собственный Rigidbody.
    /// </summary>
    public Rigidbody Rb { get; private set; }

    /// <summary>
    /// Флаг защиты от повторной нарезки за один кадр.
    /// </summary>
    private bool isSliced;

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Метод нарезки объекта на дольки или слайсы.
    /// Вызывается ножом (Knife.cs) при соприкосновении.
    /// </summary>
    /// <param name="cutDirection">Направление реза ножа.</param>
    public void Slice(Vector3 cutDirection)
    {
        if (isSliced)
            return;

        isSliced = true;

        // Воспроизводим звук и частицы (если назначены)
        if (CutSound != null)
        {
            AudioSource.PlayClipAtPoint(CutSound, transform.position, 1.0f);
        }

        if (JuiceVFX != null)
        {
            ParticleSystem vfx = Instantiate(JuiceVFX, transform.position, Quaternion.identity);
            Destroy(vfx.gameObject, 2.0f);
        }

        Vector3 spawnCenter = transform.position;
        Quaternion spawnRotation = transform.rotation;
        Vector3 sliceAxisDirection = transform.right;

        // Размер шага между спавнящимися дольками
        float stepOffset = (SlicedPiecePrefab != null) ? 0.15f : (transform.localScale.x / SliceCount);

        for (int i = 0; i < SliceCount; i++)
        {
            // Смещение вдоль оси реза (-шаг, 0, +шаг)
            float offset = (i - (SliceCount - 1) * 0.5f) * stepOffset;
            Vector3 piecePosition = spawnCenter + sliceAxisDirection * offset;

            GameObject piece;
            
            // Кусочек/слайс от продукта
            piece = Instantiate(SlicedPiecePrefab, piecePosition, spawnRotation);
            
            if (SlicedPiecePrefab == null)
            {
                Debug.LogError("Нету префаба");
            }


            piece.name = $"{gameObject.name}_Piece_{i + 1}";

            // Навешиваем маркер InteractiveObject, чтобы игрок мог поднять кусочек через Grab (ЛКМ/ПКМ)
            if (piece.GetComponent<InteractiveObject>() == null)
            {
                piece.AddComponent<InteractiveObject>();
            }

            // Настраиваем физику кусочка (Rigidbody)
            Rigidbody pieceRb = piece.GetComponent<Rigidbody>();
            if (pieceRb == null)
            {
                pieceRb = piece.AddComponent<Rigidbody>();
            }

            if (Rb != null)
            {
                pieceRb.mass = Mathf.Max(0.05f, Rb.mass / SliceCount);
            }

            // Вектор импульса разлета
            float outwardFactor = (i - (SliceCount - 1) * 0.5f);
            Vector3 pushDirection = (sliceAxisDirection * outwardFactor).normalized;
            if (pushDirection == Vector3.zero)
            {
                pushDirection = transform.forward * 0.25f;
            }

            Vector3 finalImpulse = (pushDirection * SeparationForce) + (Vector3.up * UpwardBias);
            pieceRb.AddForce(finalImpulse, ForceMode.Impulse);

            // Легкое вращение для реалистичности
            Vector3 randomTorque = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ) * TorqueAmount;
            pieceRb.AddTorque(randomTorque, ForceMode.Impulse);
        }

        // Удаляем целый объект
        Destroy(gameObject);
    }
}