using System.Collections;
using UnityEngine;

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
    }

    public override void ObjectDrop()
    {
        Rigidbody.mass = Mass;
    }
}
