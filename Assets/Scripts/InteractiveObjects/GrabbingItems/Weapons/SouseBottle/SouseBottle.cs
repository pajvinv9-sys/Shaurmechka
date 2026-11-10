using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class SouseBottle : GrabbingItem, IActivate
{
    [Header("Настройки соуса")]
    [field: SerializeField]
    public IngredientTag SauceTag { get; private set; } = IngredientTag.Ketchup;

    [Header("Particle System")]
    [SerializeField] private ParticleSystem paintParticles;

    private Coroutine paintCoroutine;
    private bool isPainting;

    protected override void Awake()
    {
        base.Awake();
    }

    public void Activate()
    {
        if (isPainting || paintParticles == null)
            return;

        isPainting = true;
        var main = paintParticles.main;
        main.gravityModifier = Rigidbody.useGravity ? 1 : 0;

        paintParticles.Play();

        paintCoroutine = StartCoroutine(Paint());
    }

    public void StopActivate()
    {
        isPainting = false;

        if (paintCoroutine != null)
        {
            StopCoroutine(paintCoroutine);
            paintCoroutine = null;
        }

        if (paintParticles != null)
            paintParticles.Stop();
    }

    public override void ObjectDrop()
    {
        base.ObjectDrop();

        StopActivate();
    }

    public override void ObjectLost()
    {
        base.ObjectLost();

        StopActivate();
    }

    public override void ObjectFound()
    {
        base.ObjectFound();
    }

    private IEnumerator Paint()
    {
        while (isPainting)
        {
            yield return null;
        }

        paintCoroutine = null;
    }
}
