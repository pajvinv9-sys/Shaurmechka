using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseIngredient : GrabbingItem, IActivate
{
    public BaseIngredientCompatible BaseIngredientCompatible;
    public List<IngredientTag> ingredientsInside;

    public bool IsCooked { get; private set; }
    public void Activate()
    {
        if (!IsCooked)
            gameObject.AddComponent<ReadyForPeakup>();

        IsCooked = true;
    }

    public void StopActivate()
    {
        
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsCooked) return;

        if (collision.gameObject.GetComponent<BaseIngredient>())
            return;

        var ingredient = collision.gameObject.GetComponent<Ingredient>();

        if (ingredient == null)
            return;

        foreach (IngredientTag c in BaseIngredientCompatible.CompatibleIngredients)
        {
            if (c == ingredient.IngredientTag)
            {
                ingredientsInside.Add(c);
                Destroy(collision.gameObject);
            }
        }
    }
}
