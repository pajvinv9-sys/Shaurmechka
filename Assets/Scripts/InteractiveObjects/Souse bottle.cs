using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Оптимизированный скрипт банки соуса (кетчуп, горчица, майонез).
/// 
/// Вешается на: GameObject банки.
/// Требует: InteractiveObject, Rigidbody, Collider.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(InteractiveObject))]
public class SauceBottle : MonoBehaviour
{
    [Header("Точка носика и эффекты")]
    /// <summary>
    /// Точка кончика носика, откуда вылетает соус.
    /// </summary>
    [field: SerializeField]
    public Transform NozzlePoint { get; private set; }

    /// <summary>
    /// Дочерний визуальный объект модели банки.
    /// </summary>
    [field: SerializeField]
    public Transform VisualModel { get; private set; }

    /// <summary>
    /// Система частиц струи соуса.
    /// </summary>
    [field: SerializeField]
    public ParticleSystem SauceStreamVFX { get; private set; }

    /// <summary>
    /// Скорость вылета струи соуса из носика (м/с).
    /// </summary>
    [field: SerializeField]
    public float StreamSpeed { get; private set; } = 5.0f;

    /// <summary>
    /// Источник звука выжимания соуса.
    /// </summary>
    [field: SerializeField]
    public AudioSource PourAudioSource { get; private set; }

    [Header("Наклон в руках")]
    /// <summary>
    /// Фиксированный угол наклона носика вниз (в градусах).
    /// </summary>
    [field: SerializeField]
    public float TiltAngle { get; private set; } = 30.0f;

    private Rigidbody rb;
    private Camera playerCamera;
    private Grab playerGrab;
    private ParticleSystem.EmissionModule vfxEmission;

    private bool isCurrentlyHeld;
    private bool wasHeldLastFrame;
    private bool isPouring;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        playerCamera = Camera.main;
        playerGrab = FindFirstObjectByType<Grab>();

        if (NozzlePoint == null)
        {
            Transform foundNozzle = transform.Find("NozzlePoint");
            NozzlePoint = foundNozzle != null ? foundNozzle : transform;
        }

        if (VisualModel == null)
        {
            VisualModel = transform;
        }

        if (SauceStreamVFX != null)
        {
            vfxEmission = SauceStreamVFX.emission;
            vfxEmission.enabled = false;

            var main = SauceStreamVFX.main;
            main.startSpeed = StreamSpeed;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var shape = SauceStreamVFX.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 2.0f;

            SauceStreamVFX.Play();
        }

        if (PourAudioSource != null)
        {
            PourAudioSource.loop = true;
        }
    }

    private void Update()
    {
        CheckHeldState();

        if (isCurrentlyHeld)
        {
            AlignNozzleWithCamera();
            HandlePourInput();
        }
        else
        {
            if (isPouring)
            {
                StopPouring();
            }
        }
    }

    private void CheckHeldState()
    {
        if (playerGrab == null)
        {
            isCurrentlyHeld = false;
            return;
        }

        isCurrentlyHeld = (playerGrab.Item != null && playerGrab.Item.gameObject == gameObject);

        if (!wasHeldLastFrame && isCurrentlyHeld)
        {
            OnPickup();
        }
        else if (wasHeldLastFrame && !isCurrentlyHeld)
        {
            OnDrop();
        }

        wasHeldLastFrame = isCurrentlyHeld;
    }

    private void OnPickup()
    {
        // Мгновенно гасим остаточные силы от прошлого падения
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Блокируем внутреннее физическое кручение на время удержания в руках
        rb.freezeRotation = true;
    }

    private void OnDrop()
    {
        rb.freezeRotation = false;
        rb.angularVelocity = Vector3.zero;

        if (VisualModel != null && VisualModel != transform)
        {
            VisualModel.localRotation = Quaternion.identity;
        }
    }

    private void AlignNozzleWithCamera()
    {
        if (playerCamera == null)
            return;

        Vector3 cameraForward = playerCamera.transform.forward;
        Quaternion desiredRotation = Quaternion.LookRotation(cameraForward, Vector3.up) * Quaternion.Euler(TiltAngle, 0f, 0f);

        if (VisualModel != null && VisualModel != transform)
        {
            VisualModel.rotation = desiredRotation;
        }
        else
        {
            transform.rotation = desiredRotation;
        }

        if (SauceStreamVFX != null && NozzlePoint != null)
        {
            SauceStreamVFX.transform.position = NozzlePoint.position;
            SauceStreamVFX.transform.rotation = NozzlePoint.rotation;
        }
    }

    private void HandlePourInput()
    {
        bool isEKeyPressed = Keyboard.current != null && Keyboard.current.eKey.isPressed;

        if (isEKeyPressed)
        {
            if (!isPouring)
            {
                StartPouring();
            }
        }
        else
        {
            if (isPouring)
            {
                StopPouring();
            }
        }
    }

    private void StartPouring()
    {
        isPouring = true;

        if (SauceStreamVFX != null)
        {
            vfxEmission.enabled = true;
        }

        if (PourAudioSource != null && !PourAudioSource.isPlaying)
        {
            PourAudioSource.Play();
        }
    }

    private void StopPouring()
    {
        isPouring = false;

        if (SauceStreamVFX != null)
        {
            vfxEmission.enabled = false;
        }

        if (PourAudioSource != null && PourAudioSource.isPlaying)
        {
            PourAudioSource.Stop();
        }
    }
}