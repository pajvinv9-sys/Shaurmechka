using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class AttractorGun : GrabbingItem, IActivate
{
    [field: SerializeField] public float Distance { get; private set; }
    [field: SerializeField] private GameObject ray;

    public void Activate()
    {
        ray.gameObject.SetActive(true);
    }

    public void StopActivate()
    {
        ray.gameObject.SetActive(false);
    }

    public override void ObjectDrop()
    {
        base.ObjectDrop();

        StopActivate();
    }

    public override void ObjectLost()
    {
        base.ObjectLost();

        StopActivate();
    }
}
