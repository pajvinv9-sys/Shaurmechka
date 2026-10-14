/// <summary>
/// Базовый класс для ингредиентов, которые можно захватывать и готовить.
/// Наследники определяют поведение ингредиента при приготовлении.
/// </summary>
public abstract class Ingredient : GrabbingItem
{
    public override void ObjectDrop()
    {
        return;
    }

    public override void ObjectLost()
    {
        return;
    }

    /// <summary>
    /// Определяет процесс приготовления ингредиента.
    /// Реализация зависит от конкретного типа ингредиента.
    /// </summary>
    public abstract void Cook();
}
