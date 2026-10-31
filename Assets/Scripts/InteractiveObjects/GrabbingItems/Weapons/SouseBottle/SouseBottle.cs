
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class SouseBottle : GrabbingItem, IActivate
{
    [Header("Particle System")]
    [SerializeField] private ParticleSystem paintParticles;

    [Header("Painting")]
    [SerializeField] private Shader paintableShader;
    [SerializeField] private Color paintColor = Color.red;
    [SerializeField] private int textureResolution = 1024;
    [SerializeField] private float brushSize = 20f;
    [SerializeField, Range(0f, 1f)] private float brushHardness = 0.7f;

    private Coroutine paintCoroutine;
    private bool isPainting;

    private ParticleCollisionRelay collisionRelay;

    private readonly List<ParticleCollisionEvent> collisionEvents =
        new List<ParticleCollisionEvent>();

    protected override void Awake()
    {
        base.Awake();

        if (paintParticles == null)
            return;

        collisionRelay = paintParticles.GetComponent<ParticleCollisionRelay>();

        if (collisionRelay == null)
            collisionRelay = paintParticles.gameObject.AddComponent<ParticleCollisionRelay>();

        collisionRelay.Initialize(this);
    }

    public void Activate()
    {
        if (isPainting || paintParticles == null)
            return;

        isPainting = true;
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
    }

    public override void ObjectLost()
    {
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

    // Вызывается через ParticleCollisionRelay
    public void HandleParticleCollision(ParticleSystem source, GameObject other)
    {
        if (!isPainting)
            return;

        int count = source.GetCollisionEvents(other, collisionEvents);

        if (count == 0)
            return;

        Debug.Log($"[PAINT] Collisions: {count}, Object: {other.name}");

        MeshCollider meshCollider = other.GetComponent<MeshCollider>();

        if (meshCollider == null)
            meshCollider = other.GetComponentInParent<MeshCollider>();

        if (meshCollider == null)
        {
            Debug.LogWarning("[PAINT] MeshCollider not found!");
            return;
        }

        if (meshCollider.convex)
        {
            Debug.LogWarning("[PAINT] MeshCollider is Convex!");
            return;
        }

        Renderer targetRenderer = meshCollider.GetComponent<Renderer>();

        if (targetRenderer == null)
            targetRenderer = meshCollider.GetComponentInParent<Renderer>();

        if (targetRenderer == null)
        {
            Debug.LogWarning("[PAINT] Renderer not found!");
            return;
        }

        PaintableSurface surface =
            targetRenderer.GetComponent<PaintableSurface>();

        if (surface == null)
            surface = targetRenderer.gameObject.AddComponent<PaintableSurface>();

        if (!surface.IsInitialized)
        {
            Debug.Log("[PAINT] Initializing surface...");

            surface.Initialize(paintableShader, textureResolution);

            if (!surface.IsInitialized)
            {
                Debug.LogError("[PAINT] Surface initialization failed!");
                return;
            }
        }

        for (int i = 0; i < count; i++)
        {
            ParticleCollisionEvent particleEvent = collisionEvents[i];

            Vector3 point = particleEvent.intersection;
            Vector3 normal = particleEvent.normal.normalized;

            Ray ray = new Ray(point + normal * 0.05f, -normal);

            if (!meshCollider.Raycast(ray, out RaycastHit hit, 0.15f))
            {
                Debug.LogWarning("[PAINT] Raycast failed!");
                continue;
            }

            Debug.Log($"[PAINT] Hit UV: {hit.textureCoord}");

            surface.DrawPaint(
                hit.textureCoord,
                paintColor,
                brushSize,
                brushHardness
            );
        }
    }
}
