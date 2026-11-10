using System;
using UnityEngine;

/// <summary>
/// Состояния клиента.
/// </summary>
public enum CustomerState
{
    Away,           // Клиента нет на сцене
    WaitingToOrder, // Пришёл, ждёт, пока игрок примет заказ
    Talking,        // Идёт диалог с заказом
    WaitingForDish, // Заказ принят, ждёт блюдо
    Reacting        // Получил блюдо, реагирует и уходит
}

/// <summary>
/// Клиент: появляется, принимает заказ по Е, ждёт блюдо, реагирует и уходит.
/// Не знает, что такое заказ и очки: об этом сообщает событиями.
///
/// Крепится на: корневой объект клиента (он всегда активен).
/// Вся видимая часть лежит в дочернем объекте Visual, который включается и выключается.
/// </summary>
public class Customer : InteractiveObject
{
        [Header("Визуал")]
    [field: SerializeField] public GameObject Visual { get; private set; }

    [Tooltip("Контейнер с эмоциями: он поворачивается к камере.")]
    [field: SerializeField] public Transform Sprites { get; private set; }

    [Header("Эмоции")]
    [field: SerializeField] public GameObject Neutral { get; private set; }
    [field: SerializeField] public GameObject Happy { get; private set; }
    [field: SerializeField] public GameObject Angry { get; private set; }

    public CustomerState State { get; private set; } = CustomerState.Away;

    public string InteractionPrompt =>
        State == CustomerState.WaitingToOrder ? "Принять заказ" : string.Empty;

    /// <summary>Игрок выслушал заказ, клиент ждёт блюдо.</summary>
    public event Action OrderAccepted;

    /// <summary>Клиент ушёл со сцены.</summary>
    public event Action Left;

    private DialogueLine[] orderLines;
    private Camera cam;

    protected override void Awake()
    {
        base.Awake();
        Visual.SetActive(false);
    }

    private void LateUpdate()
    {
        if (State == CustomerState.Away)
            return;

        // Спрайт всегда повёрнут к камере, как в 2D-новелле.
        if (cam == null)
            cam = Camera.main;

        if (cam != null)
            Sprites.rotation = cam.transform.rotation;
    }


    /// <summary>
    /// Показывает один из трёх спрайтов эмоций, остальные скрывает.
    /// </summary>
    private void SetEmotion(GameObject emotion)
    {
        Neutral.SetActive(emotion == Neutral);
        Happy.SetActive(emotion == Happy);
        Angry.SetActive(emotion == Angry);
    }

    /// <summary>
    /// Клиент приходит и ждёт, пока игрок примет заказ.
    /// </summary>
    /// <param name="lines">Реплики с текстом заказа.</param>
    public void Arrive()
    {
        if (Visual == null) Debug.LogError("Customer: не назначен Visual", this);
        if (Neutral == null) Debug.LogError("Customer: не назначен Neutral", this);
        if (Happy == null) Debug.LogError("Customer: не назначен Happy", this);
        if (Angry == null) Debug.LogError("Customer: не назначен Angry", this);

        SetEmotion(Neutral);
        Visual.SetActive(true);
        State = CustomerState.WaitingToOrder;
    }

    public override void ObjectFound()
    {
        base.ObjectFound();

        if (State != CustomerState.WaitingToOrder)
            return;

        var speaker = LocalizationHelper.GetStringFromJSON("", "dialogs", "client");
        var greeting = LocalizationHelper.GetPhrase("greetings");
        var recipe = OrderSystem.Instance.CurrentRecipe.name.ToLower();
        Debug.Log(recipe);
        orderLines = new[]
        {
            new DialogueLine {
                Speaker = speaker,
                Text = greeting },

            new DialogueLine {
                Speaker = speaker,
                Text = LocalizationHelper.GetPhrase("order_give_me_please",$"recipe:{recipe.ToLower()}")}
        };

        // Заказ считается принятым, когда игрок пролистал диалог до конца.
        if (DialogueManager.Instance.StartDialogue(orderLines, OnOrderTold))
            State = CustomerState.Talking;
    }

    private void OnOrderTold()
    {
        State = CustomerState.WaitingForDish;
        OrderAccepted?.Invoke();

        string str = LocalizationHelper.GetItem(OrderSystem.CurrentOrder.name);
        if (str.Length > 0)
        {
            str = char.ToUpper(str[0], System.Globalization.CultureInfo.CurrentCulture) + str.Substring(1);
        }
        UIMessage.Show("Текущий заказ: " + str, -1);

    }

    /// <summary>
    /// Клиент реагирует на блюдо и уходит.
    /// </summary>
    /// <param name="happy">true, если клиент доволен.</param>
    public void React(bool happy)
    {
        State = CustomerState.Reacting;
        SetEmotion(happy ? Happy : Angry);

        DialogueLine[] lines =
        {
            new DialogueLine
            {
                Speaker = LocalizationHelper.GetStringFromJSON("", "dialogs", "client"),
                Text = happy ? LocalizationHelper.GetPhrase("thanks") : LocalizationHelper.GetPhrase("failure_order")
            }
        };

        // Если диалог по какой-то причине не запустился, клиент просто уходит.
        if (!DialogueManager.Instance.StartDialogue(lines, Leave))
            Leave();
    }

    private void Leave()
    {
        Visual.SetActive(false);
        State = CustomerState.Away;
        Left?.Invoke();
    }

    public override void ObjectSighted()
    {
        if (State == CustomerState.WaitingToOrder)
        {
            UIMessage.ShowHelp(HelpTextOnSighted);
        }
    }
}
