using UnityEngine;
using System.Collections.Generic;

public class RecipeBook : MonoBehaviour
{
    public enum Base
    {
        SampleLavash,
        CheeseLavash,
        GarlicLavash,

        BurgerBun,

        Dish
    }

    public enum Ingredients
    {
        Tomato,
        Cucumber,
        Cabbage,

        Cutlet,

        Ketchup,
        Mustard,
        Mayonnaise
    }

    public enum SpecialTouch
    {
        None,
        BurgerBun,
        BayLeaf
    }

    [System.Serializable]
    public class Recipe
    {
        public string Name;
        public Base Base;
        public List<Ingredients> Ingredients;
        public SpecialTouch SpecialTouch;
    }

    public List<Recipe> recipes;



}
