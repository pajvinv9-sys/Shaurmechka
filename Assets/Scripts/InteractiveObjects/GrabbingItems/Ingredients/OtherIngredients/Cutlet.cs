using UnityEngine;

/// <summary>
/// Реализует поведение котлеты, которую можно захватывать и готовить.
/// После приготовления меняет материал на заданный в Inspector.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class Cutlet : Ingredient
{
    [Header("Настройки готовки")]
    [Tooltip("Материал, который применится к объекту после жарки")]
    [SerializeField] private Material cookedMaterial;

    public bool IsCooked { get; private set; }
    private MeshRenderer meshRenderer;

    protected override void Awake()
    {
        base.Awake();

        meshRenderer = GetComponent<MeshRenderer>();
    }

    public override void Cook()
    {
        // Не допускаем повторного приготовления и проверяем наличие материала.
        if (IsCooked || cookedMaterial == null) return;

        // Меняем внешний вид котлеты и сохраняем состояние готовности.
        meshRenderer.material = cookedMaterial;
        IsCooked = true;
    }

    public override void ObjectDrop()
    {
        base.ObjectDrop();

        BasketballScore.SetValuesOnThrow(Grab.Player.GetComponent<Rigidbody>());
    }
    public override void ObjectLost()
    {
        base.ObjectLost();

        BasketballScore.SetValuesOnThrow(Grab.Player.GetComponent<Rigidbody>(), 0.5f);
    }
}
