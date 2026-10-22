    using System.Collections;
    using UnityEngine;
    using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    // Скорость перемещения персонажа.
    [field: SerializeField] public float Speed { get; private set; } = 5f;
    [field: SerializeField] public float RunModify { get; private set; } = 5f;
    [field: SerializeField] public int RunFieldOfView { get; private set; } = 65;
    [field: SerializeField] public int WalkFieldOfView { get; private set; } = 60;
    public Rigidbody Rigidbody { get; private set; }

    // Действие Input System, отвечающее за ввод направления движения.
    [field: SerializeField] public InputAction MovementKeys { get; private set; }
    [field: SerializeField] public InputAction RunKey { get; private set; }

    private Vector2 _moveInput;
    private float speedNow;
    private Coroutine _cameraCoroutine;
    [field: SerializeField] public Camera Camera { get; private set; }

    private void Awake()
    {
        speedNow = Speed;

        if (Rigidbody == null)
        {
            Rigidbody = GetComponent<Rigidbody>();

            if (Rigidbody == null)
            {
                Debug.LogError("Rigidbody не найден");
            }
        }

        if (MovementKeys.bindings.Count == 0)
        {
            Debug.LogError("MovementKeys не назначен");
        }
    }

    private void OnEnable()
    {
        // Подписываемся на изменение ввода и включаем обработку действия.
        MovementKeys.Enable();
        MovementKeys.performed += OnMovement;
        MovementKeys.canceled += OnMovement;

        RunKey.Enable();
        RunKey.performed += OnRun;
        RunKey.canceled += OnWalk;
    }

    private void OnDisable()
    {
        // Удаляем подписки, чтобы избежать повторных вызовов обработчика.
        MovementKeys.performed -= OnMovement;
        MovementKeys.canceled -= OnMovement;
        MovementKeys.Disable();

        RunKey.performed -= OnRun;
        RunKey.canceled -= OnWalk;
        RunKey.Disable();
    }

    private void OnMovement(InputAction.CallbackContext context)
    {
        // Сохраняем направление ввода; нормализация исключает ускорение по диагонали.
        _moveInput = context.ReadValue<Vector2>().normalized;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        // Переводим ввод в направление относительно поворота персонажа.
        Vector3 moveDirection =
            transform.right * _moveInput.x +
            transform.forward * _moveInput.y;

        moveDirection.Normalize();

        // Меняем скорость по горизонтальным осям, сохраняя вертикальную скорость,
        // чтобы не мешать гравитации и прыжкам.
        Vector3 velocity = Rigidbody.linearVelocity;

        velocity.x = moveDirection.x * speedNow;
        velocity.z = moveDirection.z * speedNow;

        Rigidbody.linearVelocity = velocity;
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        speedNow = Speed * RunModify;
        ChangeCameraFOV(RunFieldOfView);
    }

    private void OnWalk(InputAction.CallbackContext context)
    {
        speedNow = Speed;
        ChangeCameraFOV(WalkFieldOfView);
    }

    private void ChangeCameraFOV(int to)
    {
        if (_cameraCoroutine != null)
        {
            StopCoroutine(_cameraCoroutine);
        }

        _cameraCoroutine = StartCoroutine(ChangeFieldOfCamera(to));
    }

    private IEnumerator ChangeFieldOfCamera(float to)
    {
        float speed = 40f;

        while (!Mathf.Approximately(Camera.fieldOfView, to))
        {
            Camera.fieldOfView = Mathf.MoveTowards(
                Camera.fieldOfView,
                to,
                speed * Time.deltaTime
            );

            yield return null;
        }

        Camera.fieldOfView = to;
        _cameraCoroutine = null;
    }
}
