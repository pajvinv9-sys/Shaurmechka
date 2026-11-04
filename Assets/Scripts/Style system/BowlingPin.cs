using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BowlingPin : MonoBehaviour
{
    [Tooltip("Минимальная скорость шара для зачёта удара.")]
    [field: SerializeField] public float MinHitSpeed { get; private set; } = 1.5f;

    public bool IsKnockedDown { get; private set; }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsKnockedDown) return;

        if (collision.gameObject.TryGetComponent<BoulingBall>(out var ball))
        {
            if (ball.Rigidbody != null && ball.Rigidbody.linearVelocity.magnitude >= MinHitSpeed)
            {
                IsKnockedDown = true;

                if (BowlingScore.Instance != null)
                {
                    BowlingScore.Instance.RegisterPinHit();
                }
            }
        }
    }

    public void ResetPin()
    {
        IsKnockedDown = false;
    }
}
