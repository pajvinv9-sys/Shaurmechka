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

    private bool isCooked = false;
    private MeshRenderer meshRenderer;

    protected override void Awake()
    {
        base.Awake();

        meshRenderer = GetComponent<MeshRenderer>();
    }

    public override void Cook()
    {
        // Не допускаем повторного приготовления и проверяем наличие материала.
        if (isCooked || cookedMaterial == null) return;

        // Меняем внешний вид котлеты и сохраняем состояние готовности.
        meshRenderer.material = cookedMaterial;
        isCooked = true;
    }
}
