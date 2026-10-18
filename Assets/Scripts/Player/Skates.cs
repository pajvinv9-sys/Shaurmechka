using UnityEngine;
using UnityEngine.InputSystem;

public class Skates : MonoBehaviour
{
    // Скорость перемещения персонажа.
    [field: SerializeField] public float ForwardSpeed { get; private set; } = 4f;
    [field: SerializeField] public float AroundSpeed { get; private set; } = 2f;
    [field: SerializeField] public InputAction MovementKeys { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
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
        Vector3 forwardDir = transform.forward * ForwardSpeed;

        Vector3 aroundForwardDir =
            transform.forward * (_moveInput.y * AroundSpeed);

        Vector3 aroundSideDir =
            transform.right * (_moveInput.x * AroundSpeed);

        forwardDir.y = Rigidbody.linearVelocity.y;

        Rigidbody.linearVelocity =
            forwardDir + aroundForwardDir + aroundSideDir;
    }
}
