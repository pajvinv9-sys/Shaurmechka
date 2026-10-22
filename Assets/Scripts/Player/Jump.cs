using UnityEngine;
using UnityEngine.InputSystem;

public class Jump : MonoBehaviour
{
    // Действие Input System, запускающее прыжок.
    [field: SerializeField] public InputAction JumpButton { get; private set; }

    [field: SerializeField] public float JumpForce { get; private set; }

    // Если включено, прыжок доступен даже без контакта с землёй.
    [field: SerializeField] public bool OverJump { get; private set; }

    private Rigidbody rb;
    [field: SerializeField] public bool CanJump { get; private set; }

    public bool InJump { get; private set; }

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
        // Прыжок разрешён, если персонаж на земле или включён OverJump.
        if (OverJump || CanJump)
        {
            rb.AddForce(0, JumpForce, 0);
            InJump = true;
            CanJump = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Проверяем, что персонаж столкнулся с объектом, помеченным компонентом Ground.
        if (collision.gameObject.GetComponent<Ground>() != null)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                // Проверяем, что поверхность находится под персонажем, а не сбоку.
                if (contact.normal.y > 0.5f)
                {
                    CanJump = true;
                    InJump = false;
                    break;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<Ground>() != null)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                // Сбрасываем возможность прыжка при потере контакта с землёй.
                if (contact.normal.y > 0.5f)
                {
                    CanJump = false;
                    InJump = true;
                    break;
                }
            }
        }
    }
}
