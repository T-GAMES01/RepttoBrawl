using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private InputHandler input; // drag script reference here (not GameObject)

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    private bool isGrounded;
    private int jumpCount = 0;       // track jumps
    public int maxJumps = 2;         // 2 = double jump

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

        // ✅ movement
        rb.linearVelocity = new Vector2(input.MoveInput.x * moveSpeed, rb.linearVelocity.y);

        // ✅ jump logic
        if (input.JumpPressed && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;
            input.ResetJump(); // jump consume ho gaya
            Debug.Log($"🟢 Jump #{jumpCount}");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
            jumpCount = 0; // ✅ reset jump count on landing
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            isGrounded = false;
    }
}
