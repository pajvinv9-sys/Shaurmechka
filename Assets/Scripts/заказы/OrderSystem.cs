using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

/// <summary>
/// Управляет циклом заказов: вызывает клиента через случайный интервал,
/// генерирует заказ, проверяет сданное блюдо и считает очки.
///
/// Крепится на: отдельный объект сцены (OrderSystem).
/// </summary>
public class OrderSystem : MonoBehaviour
{
    [Header("Ссылки")]
    [field: SerializeField] public Customer Customer { get; private set; }
    [field: SerializeField] public DeliveryZone Zone { get; private set; }

    [Header("Появление клиентов")]
    [field: SerializeField] public float MinDelay { get; private set; } = 5f;
    [field: SerializeField] public float MaxDelay { get; private set; } = 15f;

    [Header("Реакция клиента")]
    [Tooltip("Доля выполненного заказа, начиная с которой клиент доволен.")]
    [field: SerializeField] public float HappyThreshold { get; private set; } = 0.5f;

    /// <summary>
    /// Текущий заказ: список тегов, которые должны быть в блюде.
    /// </summary>
    private Recipe recipe;

    /// <summary>
    /// Вызывается после проверки блюда с итоговыми очками (будущий ScoreManager подпишется сюда).
    /// </summary>
    public event Action<int> PointsAwarded;

    private float startTime;

    private void OnEnable()
    {
        Customer.OrderAccepted += OnOrderAccepted;
        Customer.Left += ScheduleNextCustomer;
        Zone.Delivered += OnDelivered;
        Zone.DeliveredIngredient += OnDelivered;
    }

    private void OnDisable()
    {
        Customer.OrderAccepted -= OnOrderAccepted;
        Customer.Left -= ScheduleNextCustomer;
        Zone.Delivered -= OnDelivered;
        Zone.DeliveredIngredient -= OnDelivered;
    }

    private void Start()
    {
        ScheduleNextCustomer();
    }

    private void ScheduleNextCustomer()
    {
        // Пока заказа нет, зона блюда не принимает.
        Zone.IsAccepting = false;
        StartCoroutine(CustomerArrivesAfterDelay());
    }

    private IEnumerator CustomerArrivesAfterDelay()
    {
        GenerateOrder();
        Customer.Arrive(BuildOrderLines());

        yield return new WaitForSeconds(UnityEngine.Random.Range(MinDelay, MaxDelay));
    }

    /// <summary>
    /// Создаёт случайный заказ
    /// </summary>
    private void GenerateOrder()
        => recipe = RecipesInGame.GetRandomRecipe();

    /// <summary>
    /// Собирает реплики с текстом заказа. Тексты пока заглушки.
    /// </summary>
    private DialogueLine[] BuildOrderLines()
    {
        var counttext = "";
        if (LocalizationHelper.GetGender(recipe.name, "ru") == "feminine")
            counttext = "одну";
        else counttext = "один";

            return new[]
            {
            new DialogueLine { Speaker = "Клиент", Text = "Здравствуйте!" },
            new DialogueLine { Speaker = "Клиент", Text = $"Мне {counttext} {LocalizationHelper.GetItem(recipe.name, "ru", true, "accusative")}, пожалуйста"}
        };
    }

    private void OnOrderAccepted()
    {
        startTime = Time.time;
        Zone.IsAccepting = true;
    }

    private void OnDelivered(BaseIngredient baseIngredient)
    {
        // Больше блюд не принимаем, пока не придёт следующий клиент.
        Zone.IsAccepting = false;

        var pool = recipe.OtherIngredients;
        int matched = 0;
        foreach(var c in recipe.OtherIngredients)
        {
            if (pool.Remove(c))
                matched++;

            float fraction = baseIngredient.ingredientsInside.Count == 0 ? 1f : (float)matched / baseIngredient.ingredientsInside.Count;
            float points = Mathf.Lerp(-recipe.FailPenalty, recipe.MaxPoints, fraction);
            float timeFactor = 1f - Mathf.Clamp01((Time.time - startTime) / recipe.BonusTime);
            points += recipe.MaxSpeedBonus * fraction * timeFactor;
            int total = Mathf.RoundToInt(points);
            Debug.Log(
            "Заказ: " + string.Join(", ", recipe.name) +
            " | состав: " + string.Join(", ", baseIngredient.ingredientsInside) +
            " | очки: " + total);

            PointsAwarded?.Invoke(total);
            Customer.React(fraction >= HappyThreshold);
        }
    }

    private void OnDelivered(Ingredient ingredient)
    {
        Zone.IsAccepting = false;
        Debug.Log(ingredient.IngredientTag + " " + recipe.OtherIngredients[0]);
        if (recipe.OtherIngredients.Contains(ingredient.IngredientTag))
        {
            float timeFactor = 1f - Mathf.Clamp01((Time.time - startTime) / recipe.BonusTime);
            var points = recipe.MaxPoints + recipe.MaxSpeedBonus * timeFactor;
            var total = Mathf.RoundToInt(points);
            Debug.Log(
            "Заказ: " + string.Join(", ", recipe.name) +
            " | очки: " + total);

            PointsAwarded?.Invoke(total);
            Customer.React(true);
        }
        else
        {
            var total = -recipe.FailPenalty;
            Debug.Log(
            "Заказ: " + string.Join(", ", recipe.name) +
            " | очки: " + total);

            PointsAwarded?.Invoke(total);
            Customer.React(false);
        }
    }
}
