using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Управляет захватом, перемещением, вращением и отпусканием интерактивных объектов.
/// Скрипт крепится к камере, дочерней к игроку. Rotate должен находиться на игроке.
/// </summary>
public class Grab : MonoBehaviour
{
    [Header("Управление")]
    [field: SerializeField] public InputAction DropKey { get; private set; }
    [field: SerializeField] public InputAction ActionKey { get; private set; }
    [field: SerializeField] public InputAction RotateObjectKey { get; private set; }

    [Header("Параметры перемещения")]
    [field: SerializeField] public float Distance { get; private set; }

    [Header("Параметры вращения")]
    [field: SerializeField] public float RotationSpeed { get; private set; } = 5f;
    [field: SerializeField] public float ObjectRotationSensitivity { get; private set; } = 10f;
    [field: SerializeField] public float ObjectRotationSmoothness { get; private set; } = 12f;

    [Header("Параметры броска")]
    [field: SerializeField] public float DropForce { get; private set; } = 5f;



    public GrabbingItem Item { get; private set; }
    public IActivate ItemActivate { get; private set; }
    public LaunchBeam LaunchBeam { get; private set; }
    


    private Vector3 grabPoint;
    private Coroutine takeCoroutine;
    private bool isRotating;
    private bool previousFixedCamera;
    private Quaternion targetObjectRotation;
    private Quaternion cameraRotationOffset;
    private Rotate Rotate;
    private bool didGrabbingItemHaveGravity;
    private bool wasRotate;
    private bool rotationInitialized;
    private Quaternion initialRotationTarget;

    public static Grab Instance;
    [field: SerializeField] private Transform player;

    public static Transform Player => Instance.player;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (LaunchBeam == null)
            LaunchBeam = GetComponent<LaunchBeam>();

        // Grab находится на камере, а Rotate — на родительском объекте игрока.
        if (Rotate == null)
            Rotate = GetComponentInParent<Rotate>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnEnable()
    {
        if (LaunchBeam != null && LaunchBeam.BeamKey != null)
            LaunchBeam.BeamKey.canceled += OnEndTake;

        DropKey.started += Drop;
        DropKey.Enable();

        ActionKey.started += Activate;
        ActionKey.canceled += StopActivate;
        ActionKey.Enable();

        RotateObjectKey.started += StartObjectRotation;
        RotateObjectKey.canceled += StopObjectRotation;
        RotateObjectKey.Enable();
    }

    private void OnDisable()
    {
        if (LaunchBeam != null && LaunchBeam.BeamKey != null)
            LaunchBeam.BeamKey.canceled -= OnEndTake;

        DropKey.started -= Drop;
        DropKey.Disable();

        ActionKey.started -= Activate;
        ActionKey.canceled -= StopActivate;
        ActionKey.Disable();

        RotateObjectKey.started -= StartObjectRotation;
        RotateObjectKey.canceled -= StopObjectRotation;
        RotateObjectKey.Disable();

        StopTake();
    }

    /// <summary>
    /// Начинает удержание объекта.
    /// </summary>
    public void StartTake(GrabbingItem interactiveObject)
    {
        if (interactiveObject == null || interactiveObject.Rigidbody == null)
            return;

        StopTake();

        Item = interactiveObject;
        ItemActivate = Item.GetComponent<IActivate>();

        Rigidbody rb = Item.Rigidbody;

        // Убираем остаточное физическое вращение.
        rb.angularVelocity = Vector3.zero;

        // Сбрасываем состояние вращения.
        wasRotate = !Item.NeedRotateInStart;
        rotationInitialized = false;
        isRotating = false;

        // Рассчитываем начальный угол только один раз.
        if (Item.NeedRotateInStart)
        {
            Vector3 directionToPlayer =
                transform.position - rb.position;

            directionToPlayer.y = 0f;

            if (directionToPlayer.sqrMagnitude > 0.001f)
            {
                initialRotationTarget =
                    Quaternion.LookRotation(directionToPlayer.normalized, Vector3.up)
                    * Item.RotateInStart;
            }
            else
            {
                wasRotate = true;
            }
        }

        takeCoroutine = StartCoroutine(Take());
    }

    /// <summary>
    /// Удерживает предмет и плавно обновляет его ориентацию.
    /// </summary>
    private IEnumerator Take()
    {
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        while (Item != null)
        {
            if (Item.Rigidbody == null)
                break;

            RotateItem();

            Rigidbody rb = Item.Rigidbody;

            // Не допускаем случайного физического вращения.
            rb.angularVelocity = Vector3.zero;

            // Точка удержания перед камерой.
            grabPoint = transform.position + transform.forward * Distance;

            Vector3 positionError = grabPoint - rb.position;

            Vector3 force =
                positionError * Item.SpringInHands
                - rb.linearVelocity * Item.DampingInHands;

            rb.AddForce(force, ForceMode.Force);

            yield return wait;
        }

        takeCoroutine = null;
    }

    private void OnEndTake(InputAction.CallbackContext context)
    {
        StopTake();
    }

    /// <summary>
    /// Завершает удержание и сбрасывает состояние.
    /// </summary>
    public void StopTake()
    {
        if (takeCoroutine != null)
        {
            StopCoroutine(takeCoroutine);
            takeCoroutine = null;
        }

        if (isRotating && Rotate != null)
            Rotate.FixedCamera = previousFixedCamera;

        if (Item != null)
            Item.ObjectLost();

        Item = null;
        ItemActivate = null;

        isRotating = false;
        wasRotate = false;
        rotationInitialized = false;

        cameraRotationOffset = Quaternion.identity;
        targetObjectRotation = Quaternion.identity;
        initialRotationTarget = Quaternion.identity;
    }
    /// <summary>
    /// Отпускает предмет и придаёт ему импульс вперёд.
    /// </summary>
    private void Drop(InputAction.CallbackContext context)
    {
        if (Item == null || Item.Rigidbody == null)
            return;

        Item.Rigidbody.AddForce(
            transform.forward * DropForce * Item.LocalDropForce,
            ForceMode.Impulse
        );
        LaunchBeam.IgnoreUntilLookAway(Item);
        Item.ObjectDrop();
        StopTake();
        
    }

    private void Activate(InputAction.CallbackContext context)
    {
        ItemActivate?.Activate();
    }

    private void StopActivate(InputAction.CallbackContext context)
    {
        ItemActivate?.StopActivate();
    }

    /// <summary>
    /// Начинает вращение предмета.
    /// </summary>
    private void StartObjectRotation(InputAction.CallbackContext context)
    {
        if (Item == null || Item.Rigidbody == null || Rotate == null)
            return;

        if (!wasRotate)
            return;

        previousFixedCamera = Rotate.FixedCamera;

        isRotating = true;
        Rotate.FixedCamera = true;
    }

    private void StopObjectRotation(InputAction.CallbackContext context)
    {
        isRotating = false;

        if (Rotate != null)
            Rotate.FixedCamera = previousFixedCamera;
    }


    private void RotateItem()
    {
        Rigidbody rb = Item.Rigidbody;

        // ==========================================
        // 1. Начальный поворот.
        // ==========================================

        if (!wasRotate)
        {
            float angle = Quaternion.Angle(
                rb.rotation,
                initialRotationTarget
            );

            float maxRotation =
                RotationSpeed * Time.fixedDeltaTime;

            if (angle <= maxRotation || angle <= 0.01f)
            {
                rb.MoveRotation(initialRotationTarget);

                wasRotate = true;
                rotationInitialized = false;

                // Даём физике применить конечный угол.
                return;
            }

            rb.MoveRotation(
                Quaternion.RotateTowards(
                    rb.rotation,
                    initialRotationTarget,
                    maxRotation
                )
            );

            return;
        }

        // ==========================================
        // 2. Инициализация вращения.
        // ==========================================

        if (!rotationInitialized)
        {
            Quaternion cameraReference = Item.RotateRelativeToCamera
                ? transform.rotation
                : Quaternion.Euler(
                    0f,
                    transform.eulerAngles.y,
                    0f
                );

            cameraRotationOffset =
                Quaternion.Inverse(cameraReference) * rb.rotation;

            targetObjectRotation = rb.rotation;

            rotationInitialized = true;
        }

        // ==========================================
        // 3. Ручное вращение мышью.
        // ==========================================

        if (isRotating && Rotate != null)
        {
            Vector2 mouseDelta = Rotate.Input;

            Quaternion yaw = Quaternion.AngleAxis(
                mouseDelta.x * ObjectRotationSensitivity,
                Vector3.up
            );

            Quaternion pitch = Quaternion.AngleAxis(
                -mouseDelta.y * ObjectRotationSensitivity,
                Vector3.right
            );

            cameraRotationOffset =
                yaw * pitch * cameraRotationOffset;
        }

        // ==========================================
        // 4. Выбираем систему координат камеры.
        // ==========================================

        Quaternion cameraReferenceRotation = Item.RotateRelativeToCamera
            ? transform.rotation
            : Quaternion.Euler(
                0f,
                transform.eulerAngles.y,
                0f
            );

        targetObjectRotation =
            cameraReferenceRotation * cameraRotationOffset;

        // ==========================================
        // 5. Плавное вращение.
        // ==========================================

        float smoothFactor = 1f - Mathf.Exp(
            -ObjectRotationSmoothness * Time.fixedDeltaTime
        );

        rb.MoveRotation(
            Quaternion.Slerp(
                rb.rotation,
                targetObjectRotation,
                smoothFactor
            )
        );
    }
}
