
using System.Collections;
using UnityEngine;

public class Weightlessness : GameEvent
{
    private Rigidbody[] rigidbodies;
    private bool[] originalGravityStates;

    private Jump[] jumps;

    private Coroutine deactivateCoroutine;

    [field: SerializeField]
    private float linearVelocityInStartEvent { get; set; }

    [field: SerializeField]
    private float EventDuration { get; set; }

    public override void Activate()
    {
        // Защита от повторной активации
        if (deactivateCoroutine != null)
            StopCoroutine(deactivateCoroutine);

        Deactivate();

        rigidbodies = FindObjectsByType<Rigidbody>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        originalGravityStates = new bool[rigidbodies.Length];

        for (int i = 0; i < rigidbodies.Length; i++)
        {
            originalGravityStates[i] = rigidbodies[i].useGravity;

            rigidbodies[i].useGravity = false;
            rigidbodies[i].linearVelocity =
                Vector3.up * linearVelocityInStartEvent;
        }

        jumps = FindObjectsByType<Jump>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (Jump jump in jumps)
        {
            jump.OverJump = true;
        }

        deactivateCoroutine = StartCoroutine(
            WaitToDeactivate(EventDuration)
        );
    }

    public override void Deactivate()
    {
        if (rigidbodies != null)
        {
            for (int i = 0; i < rigidbodies.Length; i++)
            {
                if (rigidbodies[i] != null)
                {
                    rigidbodies[i].useGravity =
                        originalGravityStates[i];
                }
            }
        }

        rigidbodies = null;
        originalGravityStates = null;

        if (jumps != null)
        {
            foreach (Jump jump in jumps)
            {
                if (jump != null)
                    jump.OverJump = false;
            }
        }

        jumps = null;

        deactivateCoroutine = null;
    }

    IEnumerator WaitToDeactivate(float time)
    {
        yield return new WaitForSeconds(time);

        Deactivate();
    }
}
