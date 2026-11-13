using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class AttractorGun : GrabbingItem, IActivate
{
    [field: SerializeField] public float Distance { get; private set; }
    [field: SerializeField] private GameObject ray;
    private readonly RaycastHit[] hits = new RaycastHit[16];

    private bool isActivated;
    private RaycastHit hit;
    private Coroutine searching;
    private GrabbingItem target;
    public void Activate()
    {
        ray.SetActive(true);
        isActivated = true;
    }

    public void StopActivate()
    {
        ray.SetActive(false);
        isActivated = false;
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


    private void Update()
    {
        if (isActivated)
        {
            target = FindTarget();

            if (target != null)
            {
                StopActivate();
                Grab.Instance.StartTake(target);
            }
               
        }
    }

    private GrabbingItem FindTarget()
    {
        var isFound = Physics.Raycast(transform.position-transform.forward*0.5f, -transform.forward, out hit, Distance);

        Debug.Log(isFound);
        if (!isFound) 
            return null;

        Debug.Log(hit.collider.name);
        var grabbing = hit.collider.GetComponent<GrabbingItem>();

        if (grabbing == null) 
            return null;

        return grabbing;
    }
}
