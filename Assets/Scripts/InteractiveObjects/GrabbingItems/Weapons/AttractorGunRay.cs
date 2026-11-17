using UnityEngine;

public class AttractorGunRay : MonoBehaviour
{


    private void OnTriggerEnter(Collider other)
    {
        var grabbingItem = other.GetComponent<GrabbingItem>();

        if (other == null) return;

        Grab.Instance.StartTake(grabbingItem);
    }
}
