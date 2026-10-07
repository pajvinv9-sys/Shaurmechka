using UnityEngine;

/// <summary>
/// Маркер и логика сырой котлеты.
/// Вешается на префаб котлеты.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class Cutlet : MonoBehaviour
{
    [Header("Настройки готовки")]
    [Tooltip("Материал, который применится к объекту после жарки")]
    [SerializeField] private Material cookedMaterial;

    [Tooltip("Звук жарки")]
    [SerializeField] private AudioClip sizzleSound;

    private bool isCooked = false;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    /// <summary>
    /// Вызывается триггером кольца, когда котлета падает сквозь него.
    /// </summary>
    public void Cook()
    {
        if (isCooked || cookedMaterial == null) return;

        meshRenderer.material = cookedMaterial;
        isCooked = true;

        if (sizzleSound != null)
        {
            AudioSource.PlayClipAtPoint(sizzleSound, transform.position);
        }
    }
}