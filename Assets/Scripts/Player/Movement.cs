using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    // Скорость перемещения персонажа.
    [field: SerializeField] public float Speed { get; private set; } = 5f;

    public Rigidbody Rigidbody { get; private set; }

    // Действие Input System, отвечающее за ввод направления движения.
    [field: SerializeField] public InputAction MovementKeys { get; private set; }

    private Vector2 _moveInput;

    private void Awake()
    {
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
    }

    private void OnDisable()
    {
        // Удаляем подписки, чтобы избежать повторных вызовов обработчика.
        MovementKeys.performed -= OnMovement;
        MovementKeys.canceled -= OnMovement;
        MovementKeys.Disable();
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
        velocity.x = moveDirection.x * Speed;
        velocity.z = moveDirection.z * Speed;

        Rigidbody.linearVelocity = velocity;
    }
}
