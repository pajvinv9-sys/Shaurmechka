using UnityEngine;

/// <summary>
/// Скрипт-маркер и логика нарезки ингредиента.
///
/// Вешается на: GameObject ингредиента (овощ, фрукт, кубик).
/// Требует: InteractiveObject (для поднятия в руки), Collider, Rigidbody.
///
/// Основная задача:
/// - Выступает маркером ингредиента для ножа (Knife.cs).
/// - При вызове Slice() удаляет исходный объект и создает 3 равные части.
/// - Передает новым кусочкам физический импульс разлета и свойства InteractiveObject.
/// </summary>
[DisallowMultipleComponent]
public class Ingredient : MonoBehaviour
{
    /// <summary>
    /// Количество частей, на которое делится ингредиент при нарезке.
    /// </summary>
    [field: SerializeField]
    public int SliceCount { get; private set; } = 3;

    /// <summary>
    /// Максимальное количество повторных нарезок для полученных кусочков.
    /// </summary>
    [field: SerializeField]
    public int MaxCutGenerations { get; private set; } = 1;

    /// <summary>
    /// Сила физического расталкивания кусочков в стороны.
    /// </summary>
    [field: SerializeField]
    public float SeparationForce { get; private set; } = 2.5f;

    /// <summary>
    /// Сила подброса кусочков вверх при нарезке.
    /// </summary>
    [field: SerializeField]
    public float UpwardBias { get; private set; } = 0.8f;

    /// <summary>
    /// Сила случайного вращения кусочков при нарезке.
    /// </summary>
    [field: SerializeField]
    public float TorqueAmount { get; private set; } = 1.5f;

    /// <summary>
    /// Звуковой клип хруста / нарезки ингредиента.
    /// </summary>
    [field: SerializeField]
    public AudioClip CutSound { get; private set; }

    /// <summary>
    /// Префаб частиц сока или брызг при нарезке.
    /// </summary>
    [field: SerializeField]
    public ParticleSystem JuiceVFX { get; private set; }

    /// <summary>
    /// Текущее поколение нарезки данного кусочка.
    /// </summary>
    public int CurrentGeneration { get; private set; }

    /// <summary>
    /// Ссылка на собственный Rigidbody.
    /// </summary>
    public Rigidbody Rb { get; private set; }

    /// <summary>
    /// Флаг, предотвращающий повторную нарезку за один фрейм.
    /// </summary>
    private bool isSliced;
    
    /// <summary>
    /// Инициализация поколения для созданных долек.
    /// </summary>
    public void SetGeneration(int generation)
    {
        CurrentGeneration = generation;
    }

    /// <summary>
    /// Метод нарезки ингредиента на 3 равные части.
    /// Вызывается скриптом Knife при соприкосновении.
    /// </summary>
    /// <param name="cutDirection">Направление реза лезвия ножа.</param>
    public void Slice(Vector3 cutDirection)
    {
        if (isSliced)
            return;

        if (CurrentGeneration >= MaxCutGenerations)
            return;

        isSliced = true;

        // Воспроизводим звук нарезки
        if (CutSound != null)
        {
            AudioSource.PlayClipAtPoint(CutSound, transform.position, 1.0f);
        }

        // Создаем визуальный эффект сока
        if (JuiceVFX != null)
        {
            ParticleSystem vfx = Instantiate(JuiceVFX, transform.position, Quaternion.identity);
            Destroy(vfx.gameObject, 2.0f);
        }

        Vector3 originalScale = transform.localScale;
        Vector3 originalPosition = transform.position;
        Quaternion originalRotation = transform.rotation;

        // Делим размер по локальной оси X на 3 равные части
        Vector3 newScale = originalScale;
        newScale.x /= SliceCount;
        float pieceSize = originalScale.x / SliceCount;
        Vector3 sliceAxisDirection = transform.right;

        for (int i = 0; i < SliceCount; i++)
        {
            // Смещение вдоль оси X: (-pieceSize, 0, +pieceSize)
            float offset = (i - (SliceCount - 1) * 0.5f) * pieceSize;
            Vector3 spawnPosition = originalPosition + sliceAxisDirection * offset;

            // Создаем копию текущего объекта
            GameObject piece = Instantiate(gameObject, spawnPosition, originalRotation);
            piece.name = $"{gameObject.name}_Piece_{i + 1}";
            piece.transform.localScale = newScale;

            // Настраиваем компонент Ingredient на новом кусочке
            Ingredient pieceIngredient = piece.GetComponent<Ingredient>();
            if (pieceIngredient != null)
            {
                pieceIngredient.SetGeneration(CurrentGeneration + 1);
            }

            // Убеждаемся, что на кусочке есть маркер InteractiveObject, чтобы игрок мог его поднять
            if (piece.GetComponent<InteractiveObject>() == null)
            {
                piece.AddComponent<InteractiveObject>();
            }

            // Настраиваем физику кусочка
            Rigidbody pieceRb = piece.GetComponent<Rigidbody>();
            if (pieceRb == null)
            {
                pieceRb = piece.AddComponent<Rigidbody>();
            }

            if (Rb != null)
            {
                pieceRb.mass = Mathf.Max(0.05f, Rb.mass / SliceCount);
            }

            // Импульс расталкивания долек в стороны и вверх
            float outwardFactor = (i - (SliceCount - 1) * 0.5f);
            Vector3 pushDirection = (sliceAxisDirection * outwardFactor).normalized;
            if (pushDirection == Vector3.zero)
            {
                pushDirection = transform.forward * 0.2f;
            }

            Vector3 finalImpulse = (pushDirection * SeparationForce) + (Vector3.up * UpwardBias);
            pieceRb.AddForce(finalImpulse, ForceMode.Impulse);

            // Случайный крутящий момент для естественного разлета
            Vector3 randomTorque = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ) * TorqueAmount;
            pieceRb.AddTorque(randomTorque, ForceMode.Impulse);
        }

        // Удаляем целый исходный объект
        Destroy(gameObject);
    }
}