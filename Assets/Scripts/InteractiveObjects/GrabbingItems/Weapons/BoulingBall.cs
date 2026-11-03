using System.Collections;
using UnityEngine;

/// <summary>
/// Реализует поведение шара для боулинга.
/// При броске или отпускании передаёт параметры движения игрока в BowlingScore.
/// </summary>
public class BoulingBall : GrabbingItem
{
    [field: SerializeField] public float Mass { get; private set; } = 1f;

    private float rayLength;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void ObjectFound()
    {
        base.ObjectFound();

        Rigidbody.mass = 1.5f;
    }

    public override void ObjectLost()
    {
        Rigidbody.mass = Mass;

        if (Grab.Player != null)
        {
            BowlingScore.SetValuesOnThrow(Grab.Player.GetComponent<Rigidbody>(), 0.5f);
        }
    }

    public override void ObjectDrop()
    {
        Rigidbody.mass = Mass;

        if (Grab.Player != null)
        {
            BowlingScore.SetValuesOnThrow(Grab.Player.GetComponent<Rigidbody>());
        }
    }
}
