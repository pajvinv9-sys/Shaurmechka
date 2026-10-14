using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Управляет видимостью и фиксацией системного курсора.
///
/// Крепится на GameObject, который присутствует в сцене при запуске.
/// Сохраняется между сценами, чтобы состояние курсора управлялось одним экземпляром.
///
/// При запуске курсор скрывается и фиксируется в центре окна.
/// </summary>
public class GameCursor : MonoBehaviour
{
    // Единственный экземпляр GameCursor, доступный из других компонентов.
    public static GameCursor Instance { get; private set; }

    // Текущее состояние фиксации курсора.
    public bool IsLocked { get; private set; }

    private void Awake()
    {
        // Удаляем дубликаты, чтобы избежать конфликтов управления курсором.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Сохраняем объект при переходе между сценами.
        DontDestroyOnLoad(gameObject);

        Lock();
    }

    private void OnDestroy()
    {
        // Сбрасываем ссылку, если уничтожается текущий экземпляр.
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
    // В редакторе позволяет переключать состояние курсора с помощью Input Action.
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
        // Переключаем курсор между зафиксированным и свободным состояниями.
        if (IsLocked)
            Unlock();
        else
            Lock();
    }
#endif
}
