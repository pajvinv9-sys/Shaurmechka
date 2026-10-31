
using UnityEngine;

public class ParticleCollisionRelay : MonoBehaviour
{
    private SouseBottle owner;
    private ParticleSystem particles;

    public void Initialize(SouseBottle bottle)
    {
        owner = bottle;
        particles = GetComponent<ParticleSystem>();
    }

    private void OnParticleCollision(GameObject other)
    {
        if (owner != null && particles != null)
        {
            owner.HandleParticleCollision(particles, other);
        }
    }
}
