using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Генерация")]
    [field: SerializeField] public List<IngredientTag> AvailableTags { get; private set; } = new List<IngredientTag>();
    [field: SerializeField] public int MinIngredients { get; private set; } = 1;
    [field: SerializeField] public int MaxIngredients { get; private set; } = 2;

    [Header("Очки")]
    [field: SerializeField] public int MaxPoints { get; private set; } = 100;
    [field: SerializeField] public int FailPenalty { get; private set; } = 50;
    [field: SerializeField] public int MaxSpeedBonus { get; private set; } = 50;
    [field: SerializeField] public float BonusTime { get; private set; } = 60f;

    [Header("Реакция клиента")]
    [Tooltip("Доля выполненного заказа, начиная с которой клиент доволен.")]
    [field: SerializeField] public float HappyThreshold { get; private set; } = 0.5f;

    /// <summary>
    /// Текущий заказ: список тегов, которые должны быть в блюде.
    /// </summary>
    public List<IngredientTag> CurrentOrder { get; private set; }

    /// <summary>
    /// Вызывается после проверки блюда с итоговыми очками (будущий ScoreManager подпишется сюда).
    /// </summary>
    public event Action<int> PointsAwarded;

    private float startTime;

    private void OnEnable()
    {
        Customer.OrderAccepted += OnOrderAccepted;
        Customer.Left += ScheduleNextCustomer;
        Zone.DishDelivered += OnDelivered;
    }

    private void OnDisable()
    {
        Customer.OrderAccepted -= OnOrderAccepted;
        Customer.Left -= ScheduleNextCustomer;
        Zone.DishDelivered -= OnDelivered;
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
        yield return new WaitForSeconds(UnityEngine.Random.Range(MinDelay, MaxDelay));

        GenerateOrder();
        Customer.Arrive(BuildOrderLines());
    }

    /// <summary>
    /// Создаёт случайный заказ без повторов тегов.
    /// </summary>
    private void GenerateOrder()
    {
        List<IngredientTag> pool = new List<IngredientTag>(AvailableTags);
        CurrentOrder = new List<IngredientTag>();

        int count = Mathf.Min(UnityEngine.Random.Range(MinIngredients, MaxIngredients + 1), pool.Count);

        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            CurrentOrder.Add(pool[index]);
            pool.RemoveAt(index);
        }
    }

    /// <summary>
    /// Собирает реплики с текстом заказа. Тексты пока заглушки.
    /// </summary>
    private DialogueLine[] BuildOrderLines()
    {
        List<string> names = new List<string>();
        foreach (IngredientTag tag in CurrentOrder)
            names.Add(TagToText(tag));

        return new[]
        {
            new DialogueLine { Speaker = "Клиент", Text = "Здравствуйте!" },
            new DialogueLine { Speaker = "Клиент", Text = "Мне шаурму с " + string.Join(" и ", names) + "." }
        };
    }

    // Название тега в творительном падеже для фразы "с ...". Дополняется вместе с enum.
    private string TagToText(IngredientTag tag)
    {
        switch (tag)
        {
            case IngredientTag.Tomato: return "помидорами";
            case IngredientTag.Cucumber: return "огурцами";
            default: return tag.ToString();
        }
    }

    private void OnOrderAccepted()
    {
        startTime = Time.time;
        Zone.IsAccepting = true;
    }

    private void OnDelivered(Dish dish)
    {
        // Больше блюд не принимаем, пока не придёт следующий клиент.
        Zone.IsAccepting = false;

        // Копия состава: каждый ингредиент блюда закрывает только один пункт заказа.
        List<IngredientTag> pool = new List<IngredientTag>(dish.Ingredients);

        int matched = 0;
        foreach (IngredientTag tag in CurrentOrder)
        {
            if (pool.Remove(tag))
                matched++;
        }

        float fraction = CurrentOrder.Count == 0 ? 1f : (float)matched / CurrentOrder.Count;

        // Ноль совпадений даёт штраф, все совпадения дают максимум, между ними плавно.
        float points = Mathf.Lerp(-FailPenalty, MaxPoints, fraction);

        // Бонус за скорость зависит от доли выполненного заказа.
        float timeFactor = 1f - Mathf.Clamp01((Time.time - startTime) / BonusTime);
        points += MaxSpeedBonus * fraction * timeFactor;

        int total = Mathf.RoundToInt(points);

        Debug.Log(
            "Заказ: " + string.Join(", ", CurrentOrder) +
            " | состав: " + string.Join(", ", dish.Ingredients) +
            " | очки: " + total);

        PointsAwarded?.Invoke(total);
        Customer.React(fraction >= HappyThreshold);
    }
}