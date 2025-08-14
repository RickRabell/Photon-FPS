using UnityEngine;

public class Movement : MonoBehaviour
{
    public float walkSpeed = 4f;
    public float sprintSpeed = 14f;
    public float maxVelocityChange = 10f;

    [Space]
    public float airControl = 0.5f;

    [Space]
    public float jumpHeight = 7f;

    private Vector2 input;
    private Rigidbody rb;

    private bool sprinting;
    private bool jumpQueued = false;
    private bool grounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input.Normalize();

        sprinting = Input.GetButton("Sprint");

        // Guardamos el intento de salto aquí para no perderlo entre Update y FixedUpdate
        if (Input.GetButtonDown("Jump"))
        {
            jumpQueued = true;
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = sprinting ? sprintSpeed : walkSpeed;
        float controlFactor = grounded ? 1f : airControl;

        // Movimiento siempre (con menor control si está en el aire)
        if (input.magnitude > 0.01f)
        {
            Vector3 moveForce = CalculateMovement(currentSpeed * controlFactor);
            rb.AddForce(moveForce, ForceMode.VelocityChange);
        }

        // Salto solo si estamos en el suelo y el jugador presionó salto
        if (grounded && jumpQueued)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpHeight, rb.linearVelocity.z);
            jumpQueued = false; // consumir salto
        }

        grounded = false; // Se restablecerá si colisiona
    }

    void OnCollisionStay(Collision collision)
    {
        grounded = true;
    }

    Vector3 CalculateMovement(float speed)
    {
        Vector3 targetVelocity = new Vector3(input.x, 0, input.y);
        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= speed;

        Vector3 velocity = rb.linearVelocity;
        Vector3 velocityChange = targetVelocity - velocity;

        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;

        return velocityChange;
    }
}
