using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Управляет вращением игрока и камеры от движения мыши.
/// Скрипт должен висеть на игроке.
/// </summary>
public class Rotate : MonoBehaviour
{
    /// <summary>
    /// Камера, являющаяся дочерним объектом игрока.
    /// </summary>
    [field: SerializeField]
    public Transform Camera { get; private set; }

    /// <summary>
    /// Input Action с binding на <Mouse>/delta.
    /// </summary>
    [field: SerializeField]
    public InputAction MouseDelta { get; private set; }

    [field: SerializeField]
    public float SensitiveX { get; private set; } = 1f;

    [field: SerializeField]
    public float SensitiveY { get; private set; } = 1f;

    [field: SerializeField]
    public float MinCameraAngle { get; private set; } = -80f;

    [field: SerializeField]
    public float MaxCameraAngle { get; private set; } = 80f;

    private float yaw;
    private float pitch;

    /// <summary>
    /// Дельта мыши, считанная за текущий кадр.
    /// </summary>
    public Vector2 Input { get; private set; }

    /// <summary>
    /// Если true, камера не вращается от мыши.
    /// </summary>
    [field: SerializeField]
    public bool FixedCamera { get; set; }

    private void Awake()
    {
        if (Camera == null)
        {
            Debug.LogError("Camera не назначена в Rotate.", this);
            enabled = false;
            return;
        }

        if (MouseDelta == null || MouseDelta.bindings.Count == 0)
        {
            Debug.LogError("MouseDelta не назначен в Rotate.", this);
            enabled = false;
            return;
        }

        yaw = transform.eulerAngles.y;
        pitch = Camera.localEulerAngles.x;

        if (pitch > 180f)
            pitch -= 360f;
    }

    private void OnEnable()
    {
        MouseDelta.Enable();
    }

    private void OnDisable()
    {
        MouseDelta.Disable();
        Input = Vector2.zero;
    }

    private void Update()
    {
        // Для Mouse Delta action должен быть привязан к <Mouse>/delta.
        Input = MouseDelta.ReadValue<Vector2>();

        if (FixedCamera)
            return;

        yaw += Input.x * SensitiveX;
        pitch -= Input.y * SensitiveY;

        pitch = Mathf.Clamp(
            pitch,
            MinCameraAngle,
            MaxCameraAngle
        );

        // Горизонтальное вращение — игрок.
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Вертикальное вращение — локальный поворот камеры.
        Camera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
