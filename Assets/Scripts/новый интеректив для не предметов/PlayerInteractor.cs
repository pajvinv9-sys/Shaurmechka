using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ищет перед игроком объект с IInteractable и вызывает его по нажатию клавиши.
///
/// Крепится на: камеру игрока (рядом с Grab).
/// Не зависит от LaunchBeam и InteractiveObject: у хватания и у диалогов отдельные лучи.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [Header("Управление")]
    [field: SerializeField] public InputAction InteractKey { get; private set; }

    [Header("Параметры поиска")]
    [field: SerializeField] public float MaxDistance { get; private set; } = 3f;
    [field: SerializeField] public LayerMask InteractMask { get; private set; } = ~0;

    /// <summary>
    /// Объект, на который игрок смотрит в данный момент (или null).
    /// Можно использовать для вывода подсказки "Нажмите Е".
    /// </summary>
    public IInteractable CurrentTarget { get; private set; }

    // Буфер для попаданий луча, чтобы не выделять память каждый кадр.
    private readonly RaycastHit[] hits = new RaycastHit[16];

    private void OnEnable()
    {
        InteractKey.started += OnInteract;
        InteractKey.Enable();
    }

    private void OnDisable()
    {
        InteractKey.started -= OnInteract;
        InteractKey.Disable();
        CurrentTarget = null;
    }

    /// <summary>
    /// Если true, поиск цели и взаимодействие отключены (например, во время диалога).
    /// Используется вместо отключения компонента: выключать его из обработчика
    /// собственного InputAction нельзя, Input System ломается.
    /// </summary>
    public bool IsBlocked { get; set; }

    private void Update()
    {
        CurrentTarget = IsBlocked ? null : FindTarget();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (IsBlocked)
            return;

        CurrentTarget?.Interact();
    }

    /// <summary>
    /// Ищет ближайший IInteractable перед камерой, игнорируя удерживаемый предмет.
    /// </summary>
    private IInteractable FindTarget()
    {
        int count = Physics.RaycastNonAlloc(
            transform.position,
            transform.forward,
            hits,
            MaxDistance,
            InteractMask,
            QueryTriggerInteraction.Ignore
        );

        // Предмет в руках висит прямо перед камерой и перекрывал бы луч.
        Transform held = Grab.Instance != null && Grab.Instance.Item != null
            ? Grab.Instance.Item.transform
            : null;

        float nearest = float.MaxValue;
        IInteractable result = null;

        for (int i = 0; i < count; i++)
        {
            RaycastHit hit = hits[i];

            if (hit.distance >= nearest)
                continue;

            if (held != null && hit.collider.transform.IsChildOf(held))
                continue;

            // Всё, что не является целью, перекрывает луч (стена между игроком и NPC).
            nearest = hit.distance;
            result = hit.collider.GetComponentInParent<IInteractable>();
        }

        return result;
    }

}