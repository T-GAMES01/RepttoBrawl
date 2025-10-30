using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private InputHandler input; // drag script reference here (not GameObject)

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;
    private bool isDashing = false;
    private float dashTime;
    private float lastDashTime = -Mathf.Infinity;

    private bool isGrounded;
    private int jumpCount = 0;
    public int maxJumps = 2;

    private int facingDirection = 1; // 1 = right, -1 = left

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (input == null) input = GetComponent<InputHandler>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void FixedUpdate()
    {
        if (input == null) return;

        // ✅ DASH LOGIC
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(facingDirection * dashSpeed, 0);
            dashTime -= Time.fixedDeltaTime;
            if (dashTime <= 0) isDashing = false;
            return;
        }

        // ✅ MOVEMENT
        float moveX = input.MoveInput.x;
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        // ✅ FACE FLIP LOGIC
        if (moveX > 0.1f)
            FaceDirection(1); // Right
        else if (moveX < -0.1f)
            FaceDirection(-1); // Left

        // ✅ JUMP LOGIC
        if (input.JumpPressed && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;
            input.ResetJump();
        }

        // ✅ DASH TRIGGER
        if (input.DashPressed && !isDashing && (Time.time - lastDashTime >= dashCooldown))
        {
            isDashing = true;
            dashTime = dashDuration;
            lastDashTime = Time.time;
            Debug.Log("⚡ Dash Start!");
        }
    }

    private void FaceDirection(int direction)
    {
        if (facingDirection != direction)
        {
            facingDirection = direction;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction; // flip sprite correctly
            transform.localScale = scale;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
            jumpCount = 0;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            isGrounded = false;
    }
}
