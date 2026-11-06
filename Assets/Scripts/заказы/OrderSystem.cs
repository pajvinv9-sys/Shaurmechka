
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Управляет циклом заказов.
/// Singleton.
/// </summary>
public class OrderSystem : MonoBehaviour
{
    public static OrderSystem Instance { get; private set; }

    [Header("Ссылки")]
    [field: SerializeField] public Customer Customer { get; private set; }
    [field: SerializeField] public DeliveryZone Zone { get; private set; }

    [Header("Появление клиентов")]
    [field: SerializeField] public float MinDelay { get; private set; } = 5f;
    [field: SerializeField] public float MaxDelay { get; private set; } = 15f;

    [Header("Реакция клиента")]
    [Tooltip("Доля выполненного заказа, начиная с которой клиент доволен.")]
    [field: SerializeField] public float HappyThreshold { get; private set; } = 0.5f;

    public Recipe CurrentRecipe { get; private set; }

    public event Action<int> PointsAwarded;

    private float startTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        Customer.OrderAccepted += OnOrderAccepted;
        Customer.Left += ScheduleNextCustomer;
        Zone.Delivered += OnDelivered;
        Zone.DeliveredIngredient += OnDelivered;
    }

    private void OnDisable()
    {
        if (Customer != null)
        {
            Customer.OrderAccepted -= OnOrderAccepted;
            Customer.Left -= ScheduleNextCustomer;
        }

        if (Zone != null)
        {
            Zone.Delivered -= OnDelivered;
            Zone.DeliveredIngredient -= OnDelivered;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        ScheduleNextCustomer();
    }

    private void ScheduleNextCustomer()
    {
        Zone.IsAccepting = false;
        StartCoroutine(CustomerArrivesAfterDelay());
    }

    private IEnumerator CustomerArrivesAfterDelay()
    {
        GenerateOrder();
        Customer.Arrive();

        yield return new WaitForSeconds(
            UnityEngine.Random.Range(MinDelay, MaxDelay)
        );
    }

    /// <summary>
    /// Создаёт случайный заказ.
    /// </summary>
    private void GenerateOrder()
    {
        CurrentRecipe = RecipesInGame.GetRandomRecipe();
    }

    private void OnOrderAccepted()
    {
        startTime = Time.time;
        Zone.IsAccepting = true;
    }

    private void OnDelivered(BaseIngredient baseIngredient)
    {
        Zone.IsAccepting = false;

        var pool = CurrentRecipe.OtherIngredients;
        int matched = 0;

        foreach (var c in CurrentRecipe.OtherIngredients)
        {
            if (pool.Remove(c))
                matched++;

            float fraction = baseIngredient.ingredientsInside.Count == 0
                ? 1f
                : (float)matched / baseIngredient.ingredientsInside.Count;

            float points = Mathf.Lerp(
                -CurrentRecipe.FailPenalty,
                CurrentRecipe.MaxPoints,
                fraction
            );

            float timeFactor = 1f - Mathf.Clamp01(
                (Time.time - startTime) / CurrentRecipe.BonusTime
            );

            points += CurrentRecipe.MaxSpeedBonus * fraction * timeFactor;

            int total = Mathf.RoundToInt(points);

            UIMessage.Show(
                "Заказ: " + string.Join(", ", CurrentRecipe.name) + '\n' +
                "| состав: " + string.Join(", ", baseIngredient.ingredientsInside) + '\n' +
                "| очки: " + total
            );

            PointsAwarded?.Invoke(total);
            Customer.React(fraction >= HappyThreshold);
        }
    }

    private void OnDelivered(Ingredient ingredient)
    {
        Zone.IsAccepting = false;

        Debug.Log(ingredient.IngredientTag + " " + CurrentRecipe.OtherIngredients[0]);

        if (CurrentRecipe.OtherIngredients.Contains(ingredient.IngredientTag))
        {
            float timeFactor = 1f - Mathf.Clamp01(
                (Time.time - startTime) / CurrentRecipe.BonusTime
            );

            var points = CurrentRecipe.MaxPoints + CurrentRecipe.MaxSpeedBonus * timeFactor;
            var total = Mathf.RoundToInt(points);

            UIMessage.Show(
                "Заказ: " + string.Join(", ", CurrentRecipe.name) + '\n' +
                "| очки: " + total
            );

            PointsAwarded?.Invoke(total);
            Customer.React(true);
        }
        else
        {
            var total = -CurrentRecipe.FailPenalty;

            UIMessage.Show(
                "Заказ: " + string.Join(", ", CurrentRecipe.name) + '\n' +
                "| очки: " + total
            );

            PointsAwarded?.Invoke(total);
            Customer.React(false);
        }
    }
}
