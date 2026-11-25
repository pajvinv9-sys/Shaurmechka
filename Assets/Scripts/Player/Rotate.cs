using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Управляет вращением игрока и камеры от мыши.
/// Скрипт должен находиться на игроке.
/// </summary>
public class Rotate : MonoBehaviour
{
    [field: SerializeField]
    public Transform CameraPivot { get; private set; }

    [field: SerializeField]
    public InputAction MouseDelta { get; private set; }

    [field: SerializeField]
    public float SensitiveX { get; private set; } = 0.1f;

    [field: SerializeField]
    public float SensitiveY { get; private set; } = 0.1f;

    [field: SerializeField]
    public float MinCameraAngle { get; private set; } = -80f;

    [field: SerializeField]
    public float MaxCameraAngle { get; private set; } = 80f;

    [field: SerializeField]
    public float SmoothSpeed { get; private set; } = 20f;

    [field: SerializeField]
    public bool FixedCamera { get; set; }

    private float yaw;
    private float pitch;

    private Vector2 mouseInput;
    private Vector2 currentInput;

    public Vector2 Input { get; private set; }

    private void Awake()
    {
        SensitiveX = UIController.LoadFloat(
            "",
            "settings",
            "SensX",
            50f
        );

        SensitiveY = UIController.LoadFloat(
            "",
            "settings",
            "SensY",
            50f
        );

        if (CameraPivot == null || MouseDelta == null)
        {
            Debug.LogError("CameraPivot или MouseDelta не назначены.", this);
            enabled = false;
            return;
        }

        yaw = transform.eulerAngles.y;
        pitch = CameraPivot.localEulerAngles.x;

        if (pitch > 180f)
            pitch -= 360f;
    }

    private void OnEnable()
    {
        if (MouseDelta != null)
            MouseDelta.Enable();
    }

    private void OnDisable()
    {
        if (MouseDelta != null)
            MouseDelta.Disable();

        mouseInput = Vector2.zero;
        currentInput = Vector2.zero;
        Input = Vector2.zero;
    }

    private void Update()
    {
        if (PauseMenuUI.IsPaused)
        {
            mouseInput = Vector2.zero;
            Input = Vector2.zero;
            return;
        }

        mouseInput = MouseDelta.ReadValue<Vector2>();
        Input = mouseInput;
    }

    private void LateUpdate()
    {
        if (PauseMenuUI.IsPaused || FixedCamera)
        {
            currentInput = Vector2.zero;
            return;
        }

        currentInput = Vector2.Lerp(
            currentInput,
            mouseInput,
            SmoothSpeed * Time.deltaTime
        );

        yaw += currentInput.x * SensitiveX;
        pitch -= currentInput.y * SensitiveY;

        pitch = Mathf.Clamp(pitch, MinCameraAngle, MaxCameraAngle);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        CameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
