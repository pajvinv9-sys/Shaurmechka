using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Recipe", menuName = "Scriptable Objects/Recipe")]
public class Recipe : ScriptableObject
{
    public BaseIngredientTag BaseIngredient;
    public List<IngredientTag> OtherIngredients;
    public EndIngredientTag SpecialIngredient;

    public int MaxPoints;
    public int FailPenalty;
    public int MaxSpeedBonus;
    public float BonusTime;
}
