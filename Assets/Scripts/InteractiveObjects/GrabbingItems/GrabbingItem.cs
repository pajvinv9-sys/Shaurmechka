using UnityEngine;

/// <summary>
/// Базовый класс для объектов, поддерживающих захват.
///
/// Крепится на: GameObject захватываемого объекта.
/// Наследуется от InteractiveObject и определяет поведение объекта при захвате.
/// Используется компонентом Grab.
/// </summary>
public abstract class GrabbingItem : InteractiveObject
{
    /// <summary>
    /// Определяет, разрешено ли захватывать объект в данный момент.
    /// </summary>
    [field: SerializeField] public bool CanBeGrab { get; set; } = true;

    /// <summary>
    /// Нужно ли автоматически поворачивать объект в начале захвата
    /// </summary>
    [field: SerializeField] public bool NeedRotateInStart { get; private set; } = false;

    /// <summary>
    /// Необходимый поворот, если NeedRotateInStart = true
    /// </summary>
    [field: SerializeField] public Quaternion RotateInStart { get; private set; }

    /// <summary>
    /// Определяет, насколько сильно захваченный объект стремится к точке захвата (коэффициент силы пружины)
    /// </summary>
    [field: SerializeField] public float SpringInHands { get; private set; } = 20f;

    /// <summary>
    /// Определяет, насколько быстро прекращаются колебания захваченного объекта.
    /// </summary>
    [field: SerializeField] public float DampingInHands { get; private set; } = 5f;

    /// <summary>
    /// Определяет, насколько далеко можно кинуть захваченный объект. Итоговая сила броска умножается также на Grab.DropForce
    /// </summary>
    [field: SerializeField] public float LocalDropForce { get; private set; } = 1f;

    [field: SerializeField] public bool RotateRelativeToCamera { get; protected set; } = true;


    [Header("Параметры вращения")]
    [field: SerializeField] public float RotationSpeed { get; private set; } = 5f;
    [field: SerializeField] public float ObjectRotationSensitivity { get; private set; } = 10f;
    [field: SerializeField] public float ObjectRotationSmoothness { get; private set; } = 12f;

    /// <summary>
    /// Масса объекта
    /// </summary>
    public float Mass 
    {
        get => Rigidbody.mass;
        private set => Rigidbody.mass = value;
    }

    /// <summary>
    /// Вызывается, когда объект становется захваченным.
    /// </summary>
    public override void ObjectFound()
    {
        base.ObjectFound();

        // Игнорируем обнаружение, если захват объекта запрещён.
        if (!CanBeGrab)
            return;

        // Передаём объект компоненту Grab для начала захвата.
        Grab.Instance.StartTake(this);
    }

    /// <summary>
    /// Вызывается один раз, когда камера отворачивается от объекта
    /// </summary>
    public virtual void ObjectLost()
    {
        UIMessage.HideHelp();
    }

    /// <summary>
    /// Вызывается один раз, когда камера поворачивается на объект
    /// </summary>
    public virtual void ObjectDrop()
    {
        UIMessage.HideHelp();
    }
}
