using UnityEngine;

public class BoulingBall : GrabbingItem 
{ 
    [field: SerializeField] public float Mass { get; private set; } = 1f;

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
        base.ObjectLost();

        Rigidbody.mass = Mass;
        if (Grab.Player != null)
        {
            BowlingScore.SetValuesOnThrow(Grab.Player.GetComponent<Rigidbody>(), 0.5f);
        }
    }

    public override void ObjectDrop()
    {
        base.ObjectDrop();

        Rigidbody.mass = Mass;
        if (Grab.Player != null)
        {
            BowlingScore.SetValuesOnThrow(Grab.Player.GetComponent<Rigidbody>());
        }
    }
}
