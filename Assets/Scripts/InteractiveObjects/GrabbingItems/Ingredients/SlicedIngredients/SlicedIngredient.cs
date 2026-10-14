using UnityEngine;

/// <summary>
/// Базовый класс для ингредиентов, которые можно нарезать на несколько частей.
/// Реализует ISliced и использует Slice() как действие приготовления.
/// </summary>
public abstract class SlicedIngredient : Ingredient, ISliced
{
    // Префаб одной части, создаваемой при нарезке.
    [field: SerializeField] public GameObject SlicedPiecePrefab { get; set; }

    // Количество частей, создаваемых при нарезке.
    [field: SerializeField] public int SliceCount { get; private set; }

    // Сила импульса, разбрасывающего части в стороны.
    [field: SerializeField] public float SeparationForce { get; private set; }

    // Дополнительный импульс вверх, чтобы части немного подлетали.
    [field: SerializeField] public float UpwardBias { get; private set; }

    // Сила случайного вращательного импульса каждой части.
    [field: SerializeField] public float TorqueAmount { get; private set; }

    // Расстояние между создаваемыми частями вдоль оси нарезки.
    [field: SerializeField] public float stepOffset { get; private set; }

    /// <summary>
    /// Для этого типа ингредиентов приготовление означает нарезку.
    /// </summary>
    public override void Cook()
    {
        Slice();
    }

    /// <summary>
    /// Создаёт отдельные части ингредиента, настраивает их физику
    /// и добавляет импульсы для визуально естественного разлёта.
    /// После создания частей исходный объект удаляется.
    /// </summary>
    public void Slice()
    {
        Vector3 spawnCenter = transform.position;
        Quaternion spawnRotation = transform.rotation;

        // Используем локальную ось X объекта как направление нарезки.
        Vector3 sliceAxisDirection = transform.right;

        for (int i = 0; i < SliceCount; i++)
        {
            // Распределяем части симметрично относительно центра исходного объекта.
            // Например, для трёх частей смещения будут: -шаг, 0 и +шаг.
            float offset = (i - (SliceCount - 1) * 0.5f) * stepOffset;
            Vector3 piecePosition = spawnCenter + sliceAxisDirection * offset;

            // Создаём часть с позицией и поворотом исходного ингредиента.
            GameObject piece = Instantiate(
                SlicedPiecePrefab,
                piecePosition,
                spawnRotation
            );

            piece.name = $"{gameObject.name}_Piece_{i + 1}";

            // Добавляем маркер интерактивности, если его нет в префабе.
            // Это позволяет другим системам распознавать часть как объект взаимодействия.
            if (piece.GetComponent<InteractiveObject>() == null)
            {
                piece.AddComponent<InteractiveObject>();
            }

            // Каждой части необходим Rigidbody для физических импульсов.
            Rigidbody pieceRb = piece.GetComponent<Rigidbody>();
            if (pieceRb == null)
            {
                pieceRb = piece.AddComponent<Rigidbody>();
            }

            // При наличии Rigidbody у исходного объекта делим его массу
            // между частями, сохраняя минимально допустимое значение.
            if (Rigidbody != null)
            {
                pieceRb.mass = Mathf.Max(0.05f, Rigidbody.mass / SliceCount);
            }

            // Чем дальше часть от центра в списке, тем сильнее она получает
            // импульс наружу вдоль оси нарезки.
            float outwardFactor = i - (SliceCount - 1) * 0.5f;
            Vector3 pushDirection = (sliceAxisDirection * outwardFactor).normalized;

            // Для центральной части направление разлёта равно нулю.
            // В этом случае задаём небольшое направление вперёд.
            if (pushDirection == Vector3.zero)
            {
                pushDirection = transform.forward * 0.25f;
            }

            // Объединяем боковой импульс разлёта с импульсом вверх.
            Vector3 finalImpulse =
                (pushDirection * SeparationForce) +
                (Vector3.up * UpwardBias);

            pieceRb.AddForce(finalImpulse, ForceMode.Impulse);

            // Добавляем случайное вращение, чтобы части двигались не одинаково.
            Vector3 randomTorque = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ) * TorqueAmount;

            pieceRb.AddTorque(randomTorque, ForceMode.Impulse);
        }

        // После создания всех частей исходный целый ингредиент больше не нужен.
        Destroy(gameObject);
    }
}
