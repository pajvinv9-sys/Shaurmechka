using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Управляет перемещением объекта по горизонтальной плоскости
/// на основе ввода с клавиатуры.
///
/// Крепится на: GameObject игрока.
/// Требует: Rigidbody.
/// Использует: Input Action с Vector2 для получения направления движения.
///
/// Основная логика:
/// - Получает направление движения из Input System.
/// - Двигает объект относительно его локальных осей.
/// - Сохраняет текущую вертикальную скорость Rigidbody.
/// - Перемещение выполняется в FixedUpdate.
/// </summary>
/// 
public class Movement : MonoBehaviour
{
    /// <summary>
    /// Скорость перемещения объекта.
    /// </summary>
    [field: SerializeField] public float Speed { get; private set; } = 5f;

    /// <summary>
    /// Rigidbody, используемый для физического перемещения объекта.
    /// Если не назначен, компонент ищется автоматически.
    /// </summary>
    public Rigidbody Rb { get; private set; }

    /// <summary>
    /// Input Action для получения направления движения.
    /// Должен возвращать Vector2.
    /// </summary>
    [field: SerializeField] public InputAction MovementKeys { get; private set; }

    private Vector2 _moveInput;

    private void Awake()
    {
        if (Rb == null)
        {
            Rb = GetComponent<Rigidbody>();

            if (Rb == null)
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
        MovementKeys.Enable();
        MovementKeys.performed += OnMovement;
        MovementKeys.canceled += OnMovement;
    }

    private void OnDisable()
    {
        MovementKeys.performed -= OnMovement;
        MovementKeys.canceled -= OnMovement;
        MovementKeys.Disable();
    }

    private void OnMovement(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>().normalized;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector3 moveDirection =
            transform.right * _moveInput.x +
            transform.forward * _moveInput.y;

        moveDirection.Normalize();

        Vector3 velocity = Rb.linearVelocity;
        velocity.x = moveDirection.x * Speed;
        velocity.z = moveDirection.z * Speed;

        Rb.linearVelocity = velocity;
    }
}