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
    // Определяет, разрешено ли захватывать объект в данный момент.
    [field: SerializeField] public bool CanBeGrab { get; set; } = true;
    [field: SerializeField] public bool NeedRotateInStart { get; private set; } = false;
    [field: SerializeField] public Quaternion RotateInStart { get; private set; }
    [field: SerializeField] public float SpringInHands { get; private set; } = 20f;
    [field: SerializeField] public float DampingInHands { get; private set; } = 5f;

    public float Mass 
    {
        get => Rigidbody.mass;
        private set => Rigidbody.mass = value;
    }

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
    /// Вызывается при прекращении выбора объекта.
    /// Реализация определяется в классе-наследнике.
    /// </summary>
    public virtual void ObjectLost()
    {
        UIMessage.HideHelp();
    }

    /// <summary>
    /// Вызывается при отпускании объекта игроком.
    /// Реализация определяется в классе-наследнике.
    /// </summary>
    public virtual void ObjectDrop()
    {
        UIMessage.HideHelp();
    }
}
