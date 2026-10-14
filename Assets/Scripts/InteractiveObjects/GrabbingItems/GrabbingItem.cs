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

    public override void ObjectFound()
    {
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
    public abstract void ObjectLost();

    /// <summary>
    /// Вызывается при отпускании объекта игроком.
    /// Реализация определяется в классе-наследнике.
    /// </summary>
    public abstract void ObjectDrop();
}
