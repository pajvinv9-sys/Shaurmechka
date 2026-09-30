using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// ”правл€ет вращением игрока и камеры от движени€ мыши.
///
///  репитс€ на: GameObject игрока.
/// ¬ертикальное вращение примен€етс€ к дочерней камере.
/// √оризонтальное вращение примен€етс€ к игроку.
/// </summary>
public class Rotate : MonoBehaviour
{
    /// <summary>
    ///  амера игрока, вращаема€ по вертикальной оси.
    /// </summary>
    [field: SerializeField] public Transform Camera { get; private set; }

    /// <summary>
    /// Input Action, содержащий дельту движени€ мыши.
    /// </summary>
    [field: SerializeField] public InputAction MouseDelta { get; private set; }

    /// <summary>
    /// „увствительность горизонтального вращени€.
    /// </summary>
    [field: SerializeField] public float SensitiveX { get; private set; } = 1f;

    /// <summary>
    /// „увствительность вертикального вращени€.
    /// </summary>
    [field: SerializeField] public float SensitiveY { get; private set; } = 1f;

    /// <summary>
    /// ћинимальный угол наклона камеры по вертикали.
    /// </summary>
    [field: SerializeField] public float MinCameraAngle { get; private set; } = -80f;

    /// <summary>
    /// ћаксимальный угол наклона камеры по вертикали.
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

        _yaw = transform.eulerAngles.y;
        _pitch = Camera.localEulerAngles.x;

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
        _yaw += input.x * SensitiveX;
        _pitch -= input.y * SensitiveY;

        _pitch = Mathf.Clamp(
            _pitch,
            MinCameraAngle,
            MaxCameraAngle
        );

        transform.rotation = Quaternion.Euler(
            0f,
            _yaw,
            0f
        );

        Camera.localRotation = Quaternion.Euler(
            _pitch,
            0f,
            0f
        );
    }
}