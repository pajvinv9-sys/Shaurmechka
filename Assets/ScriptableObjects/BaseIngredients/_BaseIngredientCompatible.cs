using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseIngredientCompatible", menuName = "Scriptable Objects/BaseIngredient")]
public class BaseIngredientCompatible : ScriptableObject
{
    public List<IngredientTag> CompatibleIngredients;
}
