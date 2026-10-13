
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Управляет состоянием системного курсора.
///
/// Крепится на GameObject, который присутствует в сцене при запуске.
///
/// При запуске:
/// - Скрывает системный курсор.
/// - Фиксирует курсор в центре окна приложения.
///
/// Сохраняется между сценами.
/// </summary>
public class GameCursor : MonoBehaviour
{
    public static GameCursor Instance { get; private set; }

    public bool IsLocked { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Lock();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Lock()
    {
        IsLocked = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Unlock()
    {
        IsLocked = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

#if UNITY_EDITOR
    [SerializeField] private InputAction unlockButton;

    private void OnEnable()
    {
        unlockButton.started += SwitchLockState;
        unlockButton.Enable();
    }

    private void OnDisable()
    {
        unlockButton.started -= SwitchLockState;
        unlockButton.Disable();
    }

    private void SwitchLockState(InputAction.CallbackContext context)
    {
        if (IsLocked)
            Unlock();
        else
            Lock();
    }
#endif
}
