
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Управляет поиском и взаимодействием с интерактивными объектами.
///
/// BeamKey:
/// - При удержании вызывает ObjectFound() один раз для каждого объекта.
///
/// Наведение:
/// - При наведении на новый объект вызывает ObjectSighted().
/// - При потере объекта вызывает ObjectDisappeared().
///
/// После броска объект нельзя повторно захватить,
/// пока игрок не отведёт от него взгляд.
///
/// Луч выпускается постоянно.
/// </summary>
public class LaunchBeam : MonoBehaviour
{
    [Header("Поиск объектов (Grab)")]

    [field: SerializeField]
    public InputAction BeamKey { get; private set; }

    [field: SerializeField]
    public float MaxLengthOfBeam { get; private set; } = 5f;


    [Header("Взаимодействие (Interact)")]

    [field: SerializeField]
    public float MaxDistance { get; private set; } = 3f;

    [field: SerializeField]
    public LayerMask InteractMask { get; private set; } = ~0;


    [Header("Состояние")]

    /// <summary>
    /// Блокирует поиск и взаимодействие.
    /// Например, во время диалога.
    /// </summary>
    public bool IsBlocked { get; set; }

    /// <summary>
    /// Объект, на который игрок смотрит в данный момент.
    /// </summary>
    public InteractiveObject CurrentTarget { get; private set; }


    private readonly RaycastHit[] hits = new RaycastHit[16];

    private InteractiveObject lastFoundObject;
    private InteractiveObject lastSightedObject;

    /// <summary>
    /// Объект, который недавно бросили.
    /// Повторный захват запрещён до потери наведения.
    /// </summary>
    private InteractiveObject recentlyDroppedObject;


    private void OnEnable()
    {
        BeamKey.Enable();
    }


    private void OnDisable()
    {
        BeamKey.Disable();

        ResetTargets();
        recentlyDroppedObject = null;
    }


    private void Update()
    {
        if (IsBlocked)
        {
            ResetTargets();
            return;
        }

        // Если игрок держит предмет, фиксируем цель на нём.
        if (Grab.Instance != null && Grab.Instance.Item != null)
        {
            InteractiveObject heldObject =
                Grab.Instance.Item.GetComponentInParent<InteractiveObject>();

            if (CurrentTarget != heldObject)
            {
                if (CurrentTarget != null)
                    CurrentTarget.ObjectDisappeared();

                CurrentTarget = heldObject;

                lastFoundObject = null;
                lastSightedObject = null;
            }

            return;
        }

        // Постоянно ищем объект перед камерой.
        InteractiveObject newTarget = FindTarget();

        if (CurrentTarget != newTarget)
        {
            if (CurrentTarget != null)
                CurrentTarget.ObjectDisappeared();

            CurrentTarget = newTarget;

            lastFoundObject = null;
            lastSightedObject = null;
        }

        // Снимаем блокировку после того, как игрок отвёл взгляд.
        if (CurrentTarget != recentlyDroppedObject)
        {
            recentlyDroppedObject = null;
        }

        if (CurrentTarget == null)
            return;

        if (BeamKey.IsPressed())
        {
            // Захват объекта выполняется только один раз.
            if (lastFoundObject != CurrentTarget)
            {
                lastFoundObject = CurrentTarget;

                if (CurrentTarget != recentlyDroppedObject)
                {
                    CurrentTarget.ObjectFound();
                }
            }

            lastSightedObject = null;
        }
        else
        {
            // Подсказка появляется только при новом наведении.
            if (lastSightedObject != CurrentTarget)
            {
                lastSightedObject = CurrentTarget;
                CurrentTarget.ObjectSighted();
            }

            lastFoundObject = null;
        }
    }


    /// <summary>
    /// Вызывается при броске объекта.
    /// Запрещает повторный захват до потери наведения.
    /// </summary>
    public void IgnoreUntilLookAway(InteractiveObject obj)
    {
        recentlyDroppedObject = obj;
    }


    /// <summary>
    /// Ищет ближайший интерактивный объект перед камерой.
    /// </summary>
    private InteractiveObject FindTarget()
    {
        float maxDistance = BeamKey.IsPressed()
            ? MaxLengthOfBeam
            : MaxDistance;

        int count = Physics.RaycastNonAlloc(
            transform.position,
            transform.forward,
            hits,
            maxDistance,
            InteractMask,
            QueryTriggerInteraction.Collide
        );
        
        Transform held = Grab.Instance != null &&
                         Grab.Instance.Item != null
            ? Grab.Instance.Item.transform
            : null;

        float nearestDistance = float.MaxValue;
        InteractiveObject result = null;

        for (int i = 0; i < count; i++)
        {
            RaycastHit currentHit = hits[i];

            if (currentHit.distance >= nearestDistance)
                continue;

            // Игнорируем предмет, который находится в руках.
            if (held != null &&
                (currentHit.collider.transform == held ||
                 currentHit.collider.transform.IsChildOf(held)))
            {
                continue;
            }

            // Ближайший объект перекрывает остальные цели,
            // даже если сам не является интерактивным.
            nearestDistance = currentHit.distance;

            result = currentHit.collider
                .GetComponentInParent<InteractiveObject>();
        }

        return result;
    }


    /// <summary>
    /// Сбрасывает текущую цель и уведомляет её об исчезновении.
    /// </summary>
    private void ResetTargets()
    {
        if (CurrentTarget != null)
        {
            CurrentTarget.ObjectDisappeared();
        }

        CurrentTarget = null;
        lastFoundObject = null;
        lastSightedObject = null;
    }
}
