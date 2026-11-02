using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Базовый класс для основы блюда (например, лаваша).
/// Принимает совместимые ингредиенты, аккуратно раскладывает их плашмя на своей поверхности,
/// а при активации заменяет себя на префаб готового блюда.
/// </summary>
public abstract class BaseIngredient : GrabbingItem, IActivate, IInteractable
{
    [Header("Настройки совместимости")]
    [field: SerializeField] public BaseIngredientTag BaseTag { get; private set; } = BaseIngredientTag.Lavash;
    public BaseIngredientCompatible BaseIngredientCompatible;
    public List<IngredientTag> ingredientsInside = new();

    [Tooltip("Обязательные ингредиенты, которые должны быть ВСЕ (помидор, огурец, котлета).")]
    [field: SerializeField] public List<IngredientTag> RequiredToFold { get; private set; } = new();

    [Tooltip("Список соусов, из которых достаточно добавить ХОТЯ БЫ ОДИН.")]
    [field: SerializeField] public List<IngredientTag> AnySauceToFold { get; private set; } = new();

    [Header("Готовое блюдо")]
    [Tooltip("Префаб готовой свёрнутой шаурмы (цилиндр).")]
    [field: SerializeField] public GameObject CookedPrefab { get; private set; }

    [Tooltip("Смещение по высоте при появлении шаурмы, чтобы она не застряла в столе.")]
    [field: SerializeField] public Vector3 SpawnOffset { get; private set; } = new Vector3(0f, 0.15f, 0f);

    [Header("Расположение начинки на лаваше")]
    [Tooltip("Максимальное расстояние от центра лаваша (в долях от радиуса), чтобы кусочки не вылезали за края.")]
    [field: SerializeField] public float MaxLocalRadius { get; private set; } = 0.3f;

    [Tooltip("Высота ингредиента над центром лаваша в мировых единицах.")]
    [field: SerializeField] public float SurfaceHeightOffset { get; private set; } = 0.015f;

    public bool IsCooked { get; private set; }

    public string InteractionPrompt =>
        !IsCooked && CanFold() ? "Завернуть (E)" : string.Empty;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void ObjectDrop()
    {
    }

    public override void ObjectLost()
    {
    }

    /// <summary>
    /// Сворачивание по клавише E, когда лаваш лежит на столе (через PlayerInteractor).
    /// </summary>
    public void Interact()
    {
        Activate();
    }

    /// <summary>
    /// Сворачивает лаваш: создаёт префаб готовой шаурмы и удаляет лаваш вместе с начинкой.
    /// </summary>
    public void Activate()
    {
        if (IsCooked)
            return;

        if (!CanFold())
        {
            UIMessage.Show("Сначала добавьте все ингредиенты и соус!");
            return;
        }

        if (CookedPrefab == null)
        {
            Debug.LogError("Не назначен CookedPrefab в " + gameObject.name, this);
            return;
        }

        // Сразу блокируем повторный вызов в этом же кадре до срабатывания Destroy
        IsCooked = true;

        // Если игрок держал лаваш в руках в момент нажатия, отпускаем его перед удалением
        bool wasInHands = Grab.Instance != null && Grab.Instance.Item == this;
        if (wasInHands)
        {
            Grab.Instance.StopTake();
        }

        // Спавним готовую шаурму с учётом поворота самого префаба
        GameObject cookedObject = Instantiate(
            CookedPrefab,
            transform.position + SpawnOffset,
            transform.rotation * CookedPrefab.transform.rotation
        );

        // Настраиваем компонент BaseIngredient на готовой шаурме для сдачи в DeliveryZone
        BaseIngredient cookedBase = cookedObject.GetComponent<BaseIngredient>();
        if (cookedBase == null)
        {
            cookedBase = (BaseIngredient)cookedObject.AddComponent(GetType());
        }

        cookedBase.SetAsCookedDish(ingredientsInside);

        // Если сворачивали в руках — сразу берём готовую шаурму в руки
        if (wasInHands && Grab.Instance != null)
        {
            Grab.Instance.StartTake(cookedBase);
        }

        // Удаляем плоский лаваш вместе со всеми дочерними ингредиентами на нём
        Destroy(gameObject);
    }

    public void StopActivate()
    {
    }

    /// <summary>
    /// Переводит заспавненный объект шаурмы в состояние готового блюда и передаёт ему состав.
    /// </summary>
    public void SetAsCookedDish(List<IngredientTag> collectedIngredients)
    {
        ingredientsInside = new List<IngredientTag>(collectedIngredients);
        IsCooked = true;

        if (GetComponent<ReadyForPeakup>() == null)
            gameObject.AddComponent<ReadyForPeakup>();
    }

    /// <summary>
    /// Проверяет наличие всех обязательных ингредиентов и хотя бы одного соуса.
    /// </summary>
    public bool CanFold()
    {
        // 1. Проверяем, что положены ВСЕ обязательные твёрдые ингредиенты
        if (RequiredToFold != null && RequiredToFold.Count > 0)
        {
            foreach (IngredientTag required in RequiredToFold)
            {
                if (!ingredientsInside.Contains(required))
                    return false;
            }
        }

        // 2. Проверяем, что налит ХОТЯ БЫ ОДИН соус из списка разрешённых
        if (AnySauceToFold != null && AnySauceToFold.Count > 0)
        {
            bool hasAnySauce = false;
            foreach (IngredientTag sauce in AnySauceToFold)
            {
                if (ingredientsInside.Contains(sauce))
                {
                    hasAnySauce = true;
                    break;
                }
            }

            if (!hasAnySauce)
                return false;
        }

        return ingredientsInside.Count > 0;
    }

    /// <summary>
    /// Проверяет совместимость тега по списку BaseIngredientCompatible.
    /// </summary>
    public bool IsCompatible(IngredientTag tag)
    {
        if (BaseIngredientCompatible == null || BaseIngredientCompatible.CompatibleIngredients == null)
            return false;

        return BaseIngredientCompatible.CompatibleIngredients.Contains(tag);
    }

    /// <summary>
    /// Вызывается из SouseBottle при попадании частиц соуса на лаваш.
    /// </summary>
    public void TryAddSauce(IngredientTag sauceTag)
    {
        if (IsCooked || !IsCompatible(sauceTag))
            return;

        if (!ingredientsInside.Contains(sauceTag))
        {
            ingredientsInside.Add(sauceTag);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsCooked)
            return;

        if (collision.gameObject.GetComponent<BaseIngredient>() != null)
            return;

        Ingredient ingredient = collision.gameObject.GetComponent<Ingredient>();
        if (ingredient == null || !IsCompatible(ingredient.IngredientTag))
            return;

        // Если игрок поднёс ингредиент лучом — отпускаем захват
        if (Grab.Instance != null && Grab.Instance.Item == ingredient)
        {
            Grab.Instance.StopTake();
        }

        ingredientsInside.Add(ingredient.IngredientTag);

        // Прикрепляем ингредиент к лавашу, выравнивая плашмя по высоте и радиусу
        AttachIngredientToLavash(ingredient);
    }

    private void AttachIngredientToLavash(Ingredient ingredient)
    {
        ingredient.CanBeGrab = false;

        if (ingredient.Collider != null)
            ingredient.Collider.enabled = false;

        if (ingredient.Rigidbody != null)
            Destroy(ingredient.Rigidbody);

        Transform itemTransform = ingredient.transform;

        // 1. Определяем габариты кусочка по осям X, Y, Z до смены родителя
        Vector3 size = itemTransform.lossyScale;
        MeshFilter meshFilter = ingredient.GetComponentInChildren<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            size = Vector3.Scale(meshFilter.sharedMesh.bounds.size, itemTransform.lossyScale);
        }

        // 2. Если самая тонкая сторона кусочка — ось X или Z, доворачиваем его на 90 градусов плашмя
        Quaternion flatCorrection = Quaternion.identity;
        if (size.x < size.y && size.x < size.z)
        {
            flatCorrection = Quaternion.Euler(0f, 0f, 90f);
        }
        else if (size.z < size.y && size.z < size.x)
        {
            flatCorrection = Quaternion.Euler(90f, 0f, 0f);
        }

        // 3. Переводим точку касания в локальные координаты лаваша и ограничиваем радиус
        Vector3 localPos = transform.InverseTransformPoint(itemTransform.position);
        Vector2 flatOffset = new Vector2(localPos.x, localPos.z);
        if (flatOffset.magnitude > MaxLocalRadius)
        {
            flatOffset = flatOffset.normalized * Random.Range(0.05f, MaxLocalRadius);
        }

        // 4. Вычисляем мировую позицию на поверхности лаваша
        float stackOffset = ingredientsInside.Count * 0.002f;
        Vector3 worldTargetPos = transform.TransformPoint(new Vector3(flatOffset.x, 0f, flatOffset.y));
        worldTargetPos += transform.up * (SurfaceHeightOffset + stackOffset);

        itemTransform.position = worldTargetPos;

        // 5. Укладываем кусочек плашмя со случайным поворотом вокруг вертикальной оси лаваша
        itemTransform.rotation = Quaternion.LookRotation(transform.forward, transform.up)
                                 * Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                                 * flatCorrection;

        // 6. Прикрепляем к лавашу с сохранением мировых координат
        itemTransform.SetParent(transform, true);

        Destroy(ingredient);
    }
}
