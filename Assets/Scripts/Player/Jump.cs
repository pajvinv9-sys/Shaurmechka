using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Jump : MonoBehaviour
{
    [field: SerializeField] public InputAction JumpButton {  get; private set; }
    [field: SerializeField] public float JumpForce { get; private set; }
    private Rigidbody rb;

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
        rb.AddForce(0, JumpForce, 0);
    }
}
