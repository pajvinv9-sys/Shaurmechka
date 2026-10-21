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
    public InputAction TakeKey => LaunchBeam.BeamKey;

    [Header("Параметры перемещения")]
    [field: SerializeField] public float Distance { get; private set; }
    [field: SerializeField] public float spring { get; private set; } = 20f;
    [field: SerializeField] public float damping { get; private set; } = 5f;

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



    public static Grab Instance;




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

        Item.Rigidbody.useGravity = false;

        // Запоминаем начальную ориентацию предмета относительно камеры.
        cameraRotationOffset =
            Quaternion.Inverse(transform.rotation) *
            Item.Rigidbody.rotation;

        targetObjectRotation = Item.Rigidbody.rotation;

        takeCoroutine = StartCoroutine(Take());
    }

    /// <summary>
    /// Удерживает предмет и плавно обновляет его ориентацию.
    /// </summary>
    private IEnumerator Take()
    {
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        var wasRotate = !Item.NeedRotateInStart;

        while (Item != null)
        {
            if (Item.Rigidbody == null)
                break;

            // Вращаем предмет мышью, пока зажата клавиша вращения.
            if (isRotating && Rotate != null)
            {
                wasRotate = true;
                Vector2 mouseDelta = Rotate.Input;

                Quaternion yaw = Quaternion.AngleAxis(
                    mouseDelta.x * ObjectRotationSensitivity,
                    Vector3.up
                );

                Quaternion pitch = Quaternion.AngleAxis(
                    -mouseDelta.y * ObjectRotationSensitivity,
                    Vector3.right
                );

                cameraRotationOffset = yaw * pitch * cameraRotationOffset;
            }

            if (wasRotate)
            {
                // Сохраняем ориентацию предмета относительно камеры.
                targetObjectRotation =
                    transform.rotation * cameraRotationOffset;

                // Плавное физическое вращение.
                Item.Rigidbody.MoveRotation(
                    Quaternion.Slerp(
                        Item.Rigidbody.rotation,
                        targetObjectRotation,
                        1f - Mathf.Exp(
                            -ObjectRotationSmoothness * Time.fixedDeltaTime
                        )
                    )
                );
            }
            else
            {
                Vector3 directionToPlayer =
                transform.position - Item.Rigidbody.position;

                directionToPlayer.y = 0f;

                if (directionToPlayer.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation =
                        Quaternion.LookRotation(directionToPlayer, Vector3.up);

                    Item.Rigidbody.MoveRotation(
                        Quaternion.RotateTowards(
                            Item.Rigidbody.rotation,
                            targetRotation,
                            RotationSpeed * Time.fixedDeltaTime
                        )
                    );
                }
            }
            
            // Точка удержания перед камерой.
            grabPoint = transform.position + transform.forward * Distance;

            Vector3 positionError = grabPoint - Item.Rigidbody.position;

            Vector3 force = positionError * spring - Item.Rigidbody.linearVelocity * damping;

            Item.Rigidbody.AddForce(force, ForceMode.Force);

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

        if (Item != null)
        {
            if (Item.Rigidbody != null)
                Item.Rigidbody.useGravity = true;

            Item.ObjectLost();
        }

        Item = null;
        ItemActivate = null;
        isRotating = false;
    }

    /// <summary>
    /// Отпускает предмет и придаёт ему импульс вперёд.
    /// </summary>
    private void Drop(InputAction.CallbackContext context)
    {
        if (Item == null || Item.Rigidbody == null)
            return;

        Item.Rigidbody.AddForce(
            transform.forward * DropForce,
            ForceMode.Impulse
        );

        Item.ObjectDrop();
        StopTake();
    }

    private void Activate(InputAction.CallbackContext context)
    {
        ItemActivate?.Activate();
    }

    /// <summary>
    /// Начинает вращение предмета.
    /// </summary>
    private void StartObjectRotation(InputAction.CallbackContext context)
    {
        if (Item == null || Item.Rigidbody == null || Rotate == null)
            return;

        previousFixedCamera = Rotate.FixedCamera;

        // Сохраняем текущую ориентацию объекта относительно камеры
        cameraRotationOffset =
            Quaternion.Inverse(transform.rotation) *
            Item.Rigidbody.rotation;

        // Начинаем вращение с текущего положения объекта
        targetObjectRotation = Item.Rigidbody.rotation;

        isRotating = true;
        Rotate.FixedCamera = true;
    }

    private void StopObjectRotation(InputAction.CallbackContext context)
    {
        isRotating = false;

        if (Rotate != null)
            Rotate.FixedCamera = previousFixedCamera;
    }
}
