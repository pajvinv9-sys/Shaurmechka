using UnityEngine;

public class GravityZone : MonoBehaviour
{
    [field: SerializeField] public bool IsActive { get; private set; }
    [field: SerializeField] public bool UsingGravityValueInTrigger { get; private set; }
    private void OnTriggerEnter(Collider other)
    {
        if (!IsActive) return;
        
        if (!other.TryGetComponent<Rigidbody>(out var rb)) return;

        if (rb.isKinematic) return;

        rb.useGravity = UsingGravityValueInTrigger;
    }
    private void OnTriggerExit(Collider other)
    {
        if (!IsActive) return;

        if (!other.TryGetComponent<Rigidbody>(out var rb)) return;

        if (rb.isKinematic) return;

        rb.useGravity = !UsingGravityValueInTrigger;
    }
}
