using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Настройка визуального объекта для конкретного ингредиента на основе.
/// </summary>
[Serializable]
public struct IngredientVisualSlot
{
    public IngredientTag Tag;

    [Tooltip("Отключённый дочерний объект внутри префаба основы.")]
    public GameObject VisualObject;
}

/// <summary>
/// Абстрактный класс для основы сборного блюда.
/// Сначала принимает один совместимый соус, затем остальные ингредиенты,
/// просто включая их заранее расставленные дочерние модели.
/// </summary>
public abstract class BaseIngredient : GrabbingItem, IActivate
{
    [Header("Настройки совместимости")]
    [field: SerializeField] public BaseIngredientTag BaseTag { get; private set; }
    public BaseIngredientCompatible BaseIngredientCompatible;
    public List<IngredientTag> ingredientsInside = new();

    [Header("Префабы и фиксированные места ингредиентов")]
    [Tooltip("Список соответствия тегов ингредиентов их заранее расставленным дочерним объектам.")]
    [SerializeField] private List<IngredientVisualSlot> ingredientVisuals = new();

    [Header("Готовое блюдо")]
    [Tooltip("Префаб готового собранного блюда.")]
    [field: SerializeField] public GameObject CookedPrefab { get; private set; }

    [Tooltip("Смещение по высоте при создании готового блюда.")]
    [field: SerializeField] public Vector3 SpawnOffset { get; private set; } = new Vector3(0f, 0.15f, 0f);

    public bool IsCooked { get; private set; }

    private bool hasSauce;

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
    /// Завершает сборку блюда: создаёт префаб готового блюда, передаёт ему список добавленных ингредиентов
    /// для последующей проверки в OrderSystem и удаляет заготовку.
    /// </summary>
    public void Activate()
    {
        if (IsCooked || ingredientsInside.Count == 0)
            return;

        if (CookedPrefab == null)
        {
            Debug.LogError("Не назначен CookedPrefab в " + gameObject.name, this);
            return;
        }

        IsCooked = true;

        bool wasInHands = Grab.Instance != null && Grab.Instance.Item == this;
        if (wasInHands)
        {
            Grab.Instance.StopTake();
        }

        GameObject cookedObject = Instantiate(
            CookedPrefab,
            transform.position + SpawnOffset,
            transform.rotation * CookedPrefab.transform.rotation
        );

        BaseIngredient cookedBase = cookedObject.GetComponent<BaseIngredient>();
        if (cookedBase == null)
        {
            cookedBase = (BaseIngredient)cookedObject.AddComponent(GetType());
        }

        cookedBase.SetAsCookedDish(ingredientsInside);

        if (wasInHands && Grab.Instance != null)
        {
            Grab.Instance.StartTake(cookedBase);
        }

        Destroy(gameObject);
    }

    public void StopActivate()
    {
    }

    public void SetAsCookedDish(List<IngredientTag> collectedIngredients)
    {
        ingredientsInside = new List<IngredientTag>(collectedIngredients);
        IsCooked = true;
    }

    private void OnParticleCollision(GameObject other)
    {
        Debug.Log("p");
        if (IsCooked || hasSauce || BaseIngredientCompatible == null || BaseIngredientCompatible.CompatibleIngredients == null)
            return;

        SouseBottle sauceBottle = other.GetComponentInParent<SouseBottle>();
        if (sauceBottle == null)
        {
            sauceBottle = other.transform.root.GetComponentInChildren<SouseBottle>();
        }

        if (sauceBottle == null)
            return;

        IngredientTag sauceTag = sauceBottle.SauceTag;
        if (!BaseIngredientCompatible.CompatibleIngredients.Contains(sauceTag))
            return;

        hasSauce = true;
        ingredientsInside.Add(sauceTag);
        SpawnVisualForTag(sauceTag, null);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsCooked || !hasSauce || BaseIngredientCompatible == null || BaseIngredientCompatible.CompatibleIngredients == null)
            return;

        if (collision.gameObject.GetComponent<BaseIngredient>() != null)
            return;

        Ingredient ingredient = collision.gameObject.GetComponent<Ingredient>();
        if (ingredient == null)
            return;

        IngredientTag tag = ingredient.IngredientTag;

        if (!BaseIngredientCompatible.CompatibleIngredients.Contains(tag) || ingredientsInside.Contains(tag))
            return;

        if (Grab.Instance != null && Grab.Instance.Item == ingredient)
        {
            Grab.Instance.StopTake();
        }

        ingredientsInside.Add(tag);
        SpawnVisualForTag(tag, ingredient);
    }

    /// <summary>
    /// Включает заранее расставленный дочерний объект и удаляет физический оригинал, упавший на лаваш.
    /// </summary>
    private void SpawnVisualForTag(IngredientTag tag, Ingredient collidedIngredient)
    {
        foreach (IngredientVisualSlot slot in ingredientVisuals)
        {
            if (slot.Tag == tag)
            {
                if (slot.VisualObject != null)
                {
                    slot.VisualObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"[BaseIngredient] Для тега {tag} не назначен VisualObject в инспекторе!");
                }

                if (collidedIngredient != null)
                {
                    Destroy(collidedIngredient.gameObject);
                }
                return;
            }
        }

        Debug.LogWarning($"[BaseIngredient] Тег {tag} не найден в списке Ingredient Visuals!");

        // Удаляем физический объект даже если забыли настроить его визуал, чтобы он не прыгал на лаваше
        if (collidedIngredient != null)
        {
            Destroy(collidedIngredient.gameObject);
        }
    }
}
