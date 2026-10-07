using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Jump : MonoBehaviour
{
    [field: SerializeField] public InputAction JumpButton {  get; private set; }
    [field: SerializeField] public float JumpForce { get; private set; }
    [field: SerializeField] public bool OverJump { get; private set; }

    private Rigidbody rb;
    private bool canJump;

    private void OnEnable()
    {
        JumpButton.Enable();
        JumpButton.started += JumpVoid;
    }
    private void OnDisable()
    {
        JumpButton.Disable();
        JumpButton.started -= JumpVoid;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void JumpVoid(InputAction.CallbackContext context)
    {
        if (OverJump || canJump)
        {
            rb.AddForce(0, JumpForce, 0);
            canJump = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<GroundMarker>() != null)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    canJump = true;
                    break;
                }
            }
        }
    }
}
