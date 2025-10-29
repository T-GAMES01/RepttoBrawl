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
    private bool isDashing = false;
    private float dashTime;

    private bool isGrounded;
    private int jumpCount = 0;
    public int maxJumps = 2;

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

        if (isDashing)
        {
            // jab tak dash chal raha hai
            rb.linearVelocity = new Vector2(transform.localScale.x * dashSpeed, 0);
            dashTime -= Time.fixedDeltaTime;
            if (dashTime <= 0) isDashing = false;
            return;
        }

        // Normal movement
        rb.linearVelocity = new Vector2(input.MoveInput.x * moveSpeed, rb.linearVelocity.y);

        // Jump
        if (input.JumpPressed && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;
            input.ResetJump();
        }

        // ✅ Dash trigger
        if (input.DashPressed && !isDashing)
        {
            isDashing = true;
            dashTime = dashDuration;
            Debug.Log("⚡ Dash Start!");
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
