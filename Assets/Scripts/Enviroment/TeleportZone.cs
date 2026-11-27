
using UnityEngine;

public class TeleportZone : MonoBehaviour
{
    public enum TeleportTriggerType
    {
        OnEnter,
        OnExit
    }

    [Header("General Settings")]
    [field: SerializeField] public bool IsActive { get; private set; } = true;
    [field: SerializeField] public TeleportTriggerType TriggerType { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private Transform[] movementTeleportPoints;

    [Header("GrabbingItem Settings")]
    [SerializeField] private Vector3 grabbingItemVolumeCenter;
    [SerializeField] private Vector3 grabbingItemVolumeSize = Vector3.one;

    private void OnTriggerEnter(Collider other)
    {
        if (TriggerType != TeleportTriggerType.OnEnter) return;

        TryTeleport(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (TriggerType != TeleportTriggerType.OnExit) return;

        TryTeleport(other);
    }

    private void TryTeleport(Collider other)
    {
        if (!IsActive) return;

        // Movement
        if (other.TryGetComponent<Movement>(out var movement))
        {
            TeleportMovement(movement);
            return;
        }

        // GrabbingItem
        if (other.TryGetComponent<GrabbingItem>(out var grabbingItem))
        {
            TeleportGrabbingItem(grabbingItem);
        }
    }

    private void TeleportMovement(Movement movement)
    {
        if (movementTeleportPoints == null || movementTeleportPoints.Length == 0)
            return;

        Transform target = movementTeleportPoints[
            Random.Range(0, movementTeleportPoints.Length)
        ];

        if (target == null) return;

        movement.transform.position = target.position;
        movement.transform.rotation = target.rotation;
    }

    private void TeleportGrabbingItem(GrabbingItem grabbingItem)
    {
        Vector3 halfSize = grabbingItemVolumeSize * 0.5f;

        Vector3 randomPosition = grabbingItemVolumeCenter + new Vector3(
            Random.Range(-halfSize.x, halfSize.x),
            Random.Range(-halfSize.y, halfSize.y),
            Random.Range(-halfSize.z, halfSize.z)
        );

        grabbingItem.transform.position = randomPosition;
    }

    private void OnDrawGizmosSelected()
    {
        // GrabbingItem teleport volume
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(grabbingItemVolumeCenter, grabbingItemVolumeSize);

        // Movement teleport points
        if (movementTeleportPoints == null) return;

        Gizmos.color = Color.cyan;

        foreach (Transform point in movementTeleportPoints)
        {
            if (point == null) continue;

            Gizmos.DrawSphere(point.position, 0.15f);
        }
    }
}
