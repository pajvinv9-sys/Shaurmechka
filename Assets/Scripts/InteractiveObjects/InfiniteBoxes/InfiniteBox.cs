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

    public override void ObjectFound()
    {
        // Создаём новый предмет в позиции и с поворотом источника.
        var item = Instantiate(ContentPrefab, transform.position, transform.rotation);

        // Сразу начинаем захват созданного предмета.
        Grab.Instance.StartTake(item);
    }
}
