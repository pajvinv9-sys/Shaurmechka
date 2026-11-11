    using System.Collections;
    using UnityEngine;
    using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    // Скорость перемещения персонажа.
    [field: SerializeField] public float Speed { get; private set; } = 5f;
    [field: SerializeField] public float RunModify { get; private set; } = 5f;
    public Rigidbody Rigidbody { get; private set; }

    // Действие Input System, отвечающее за ввод направления движения.
    [field: SerializeField] public InputAction MovementKeys { get; private set; }
    [field: SerializeField] public InputAction RunKey { get; private set; }

    private Vector2 _moveInput;
    private float speedNow;

    public bool IsWalk => Speed > 0 && !RunKey.IsPressed() && _moveInput.magnitude > 0.01f;
    public bool IsRun => Speed > 0 && RunModify > 1 && RunKey.IsPressed() && _moveInput.magnitude > 0.01f;
    public bool IsStay => _moveInput.magnitude < 0.01f;



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

        // Сбрасываем ввод и остаточную скорость, чтобы персонаж не скользил
        // после блокировки управления (например, на время диалога).
        _moveInput = Vector2.zero;
        speedNow = Speed;

        if (Rigidbody != null)
        {
            Vector3 velocity = Rigidbody.linearVelocity;
            velocity.x = 0f;
            velocity.z = 0f;
            Rigidbody.linearVelocity = velocity;
        }
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
    }

    private void OnWalk(InputAction.CallbackContext context)
    {
        speedNow = Speed;
    }
}
