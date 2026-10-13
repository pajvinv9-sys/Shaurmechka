using UnityEngine;

/// <summary>
/// ќбозначает объект как доступный дл€ захвата.
///
///  репитс€ на: GameObject, который должен поддерживать захват.
///
/// »спользуетс€:
/// - Grab Ч дл€ получени€ Rigidbody захватываемого объекта.
/// 
/// </summary>

[RequireComponent (typeof(InteractiveObject))]
public class GrabbingItem : MonoBehaviour
{
    public InteractiveObject InteractiveObject { get; private set; }
}
