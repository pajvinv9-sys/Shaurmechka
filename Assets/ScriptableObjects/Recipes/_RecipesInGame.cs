using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RecipesInGame : MonoBehaviour
{
    [field: SerializeField] private List<Recipe> recipes = new List<Recipe>();

    private void Awake()
    {
        recipes = recipes.Distinct().ToList();

        if (instance != null || instance == this)
            Destroy(gameObject);

        instance = this;
    }

    static RecipesInGame instance;

    public static List<Recipe> Recipes => instance.recipes;

    public static Recipe GetRandomRecipe()
    {
        var count = Recipes.Count;
        var number = Random.Range(0, count);

        return Recipes[number];
    }
}
