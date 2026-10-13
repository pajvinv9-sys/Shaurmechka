using UnityEngine;
using UnityEngine.InputSystem; 
public class Jump : MonoBehaviour
{
    public float jumpForce = 5f;
    private Rigidbody rb;
    
    public float rayDistance = 1.1f; 
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    public void OnJump(InputValue value)
    {
        if (value.isPressed && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, rayDistance);
    }

}