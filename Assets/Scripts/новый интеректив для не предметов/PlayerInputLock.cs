using UnityEngine;

/// <summary>
/// Блокирует управление игроком на время диалогов и других модальных окон.
///
/// Крепится на: GameObject игрока.
/// Grab намеренно не отключается: иначе удерживаемый предмет выпадет из рук.
/// </summary>
public class PlayerInputLock : MonoBehaviour
{
    public static PlayerInputLock Instance { get; private set; }

    [Header("Что блокировать")]
    [field: SerializeField] public Movement Movement { get; private set; }
    [field: SerializeField] public Rotate Rotate { get; private set; }
    [field: SerializeField] public PlayerInteractor Interactor { get; private set; }

    // Счётчик нужен, чтобы вложенные блокировки не снимали друг друга раньше времени.
    private int lockCount;

    public bool IsLocked => lockCount > 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Блокирует управление. Каждому вызову Lock должен соответствовать Unlock.
    /// </summary>
    public void Lock()
    {
        lockCount++;

        if (lockCount == 1)
            SetControlsEnabled(false);
    }

    /// <summary>
    /// Снимает одну блокировку; управление возвращается, когда снята последняя.
    /// </summary>
    public void Unlock()
    {
        if (lockCount == 0)
            return;

        lockCount--;

        if (lockCount == 0)
            SetControlsEnabled(true);
    }

    private void SetControlsEnabled(bool value)
    {
        if (Movement != null) Movement.enabled = value;
        if (Rotate != null) Rotate.enabled = value;

        // Interactor не выключаем, а блокируем флагом (см. PlayerInteractor.IsBlocked).
        if (Interactor != null) Interactor.IsBlocked = !value;
    }
}