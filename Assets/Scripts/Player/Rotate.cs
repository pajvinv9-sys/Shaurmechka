using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Управляет вращением игрока и камеры от движения мыши.
///
/// Крепится на: GameObject игрока.
/// Вертикальное вращение применяется к дочерней камере.
/// Горизонтальное вращение применяется к игроку.
/// </summary>
public class Rotate : MonoBehaviour
{
    /// <summary>
    /// Камера игрока, вращаемая по вертикальной оси.
    /// </summary>
    [field: SerializeField] public Transform Camera { get; private set; }

    /// <summary>
    /// Input Action, содержащий дельту движения мыши.
    /// </summary>
    [field: SerializeField] public InputAction MouseDelta { get; private set; }

    /// <summary>
    /// Чувствительность горизонтального вращения.
    /// </summary>
    [field: SerializeField] public float SensitiveX { get; private set; } = 1f;

    /// <summary>
    /// Чувствительность вертикального вращения.
    /// </summary>
    [field: SerializeField] public float SensitiveY { get; private set; } = 1f;

    /// <summary>
    /// Минимальный угол наклона камеры по вертикали.
    /// </summary>
    [field: SerializeField] public float MinCameraAngle { get; private set; } = -80f;

    /// <summary>
    /// Максимальный угол наклона камеры по вертикали.
    /// </summary>
    [field: SerializeField] public float MaxCameraAngle { get; private set; } = 80f;

    private float _yaw;
    private float _pitch;
    private Vector2 input;

    private void Awake()
    {
        if (Camera == null)
            Debug.LogError("Camera не найдена", this);

        if (MouseDelta == null || MouseDelta.bindings.Count == 0)
            Debug.LogError("MouseDelta не назначен", this);

        // Сохраняем текущие углы, чтобы вращение начиналось с положения объекта в сцене.
        _yaw = transform.eulerAngles.y;
        _pitch = Camera.localEulerAngles.x;

        // Преобразуем угол из диапазона 0–360 в диапазон -180–180.
        if (_pitch > 180f)
            _pitch -= 360f;
    }

    private void OnEnable()
    {
        MouseDelta.Enable();
    }

    private void OnDisable()
    {
        MouseDelta.Disable();
    }

    private void Update()
    {
        Vector2 input = MouseDelta.ReadValue<Vector2>();
        RotatePlayer(input);
    }

    private void RotatePlayer(Vector2 input)
    {
        // Горизонтальное движение мыши поворачивает игрока, вертикальное — камеру.
        _yaw += input.x * SensitiveX;
        _pitch -= input.y * SensitiveY;

        // Ограничиваем вертикальный угол, чтобы камера не вращалась слишком высоко или низко.
        _pitch = Mathf.Clamp(
            _pitch,
            MinCameraAngle,
            MaxCameraAngle
        );

        // Поворачиваем игрока только вокруг вертикальной оси.
        transform.rotation = Quaternion.Euler(
            0f,
            _yaw,
            0f
        );

        // Поворачиваем дочернюю камеру только по вертикальной оси.
        Camera.localRotation = Quaternion.Euler(
            _pitch,
            0f,
            0f
        );
    }
}
