using System.Collections;
using UnityEngine;

public class BoulingBall : GrabbingItem
{
    [field: SerializeField] public float Mass { get; private set; } = 1f;

    private float rayLength;

    protected override void Awake()
    {
        base.Awake();

        rayLength = transform.localScale.y / 2 + 0.01f;
    }

    public override void ObjectDrop()
    {
        StopAllCoroutines();
        Rigidbody.constraints = RigidbodyConstraints.None;
        Rigidbody.mass = Mass;
    }

    public override void ObjectLost()
    {
        StopAllCoroutines();
        Rigidbody.constraints = RigidbodyConstraints.None;
        Rigidbody.mass = Mass;
    }
    public override void ObjectFound()
    {
        base.ObjectFound();

        StartCoroutine(Fall());
    }

    private IEnumerator Fall()
    {
        Rigidbody.mass = float.MaxValue;

        while (true)
        {
            Ray ray = new Ray(transform.position, Vector3.down);
            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);

            if (!Physics.Raycast(ray, out hit, rayLength))
            {
                Debug.Log("Пол не найден");
                yield return new WaitForSeconds(0.1f);
            }
            else break;
        }

        Rigidbody.mass = 1;
        Rigidbody.constraints = RigidbodyConstraints.FreezePositionY;
    }

    
}
