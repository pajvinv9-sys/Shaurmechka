using UnityEngine;

/// <summary>
/// Бесконечный источник предметов.
/// При взаимодействии создаёт новый экземпляр заданного префаба
/// и передаёт его игроку для захвата.
/// </summary>
public class InfiniteBox : InteractiveObject
{
    // Префаб предмета, создаваемого при каждом взаимодействии.
    [field: SerializeField] public GrabbingItem ContentPrefab { get; private set; }

    private GrabbingItem lastItem;
    public override void ObjectFound()
    {
        base.ObjectFound();

        // Создаём новый предмет в позиции и с поворотом источника.
        lastItem = Instantiate(ContentPrefab, transform.position, transform.rotation);

        // Сразу начинаем захват созданного предмета.
        Grab.Instance.StartTake(lastItem);
        UIMessage.ShowHelp(lastItem.HelpTextOnFound);
    }

    public override void ObjectDisappeared()
    {
        if (Grab.Instance.Item != lastItem)
            UIMessage.HideHelp();
    }
}
