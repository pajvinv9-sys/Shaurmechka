using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Хранит состав блюда (список тегов ингредиентов).
///
/// Крепится на: корневой объект блюда (то, что игрок берёт в руки и несёт в зону сдачи).
/// Когда появится склеивание, оно просто вызывает AddIngredient.
/// </summary>
public class Dish : MonoBehaviour
{
    // Начальный состав: нужен, чтобы тестировать заказы до появления системы сборки.
    [SerializeField] private List<IngredientTag> ingredients = new List<IngredientTag>();

    public IReadOnlyList<IngredientTag> Ingredients => ingredients;

    /// <summary>
    /// Блюдо уже сдано. Защищает от повторной сдачи, если у блюда несколько коллайдеров.
    /// </summary>
    public bool IsDelivered { get; private set; }

    /// <summary>
    /// Добавляет ингредиент в состав блюда.
    /// </summary>
    /// <summary>
    /// Добавляет ингредиент в состав блюда.
    /// </summary>
    public void AddIngredient(IngredientTag tag)
    {
        ingredients.Add(tag);
    }

    public void MarkDelivered()
    {
        IsDelivered = true;
    }
}