using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    private Rigidbody2D rb;
    public InputHandler input;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void FixedUpdate()
    {
        if (input == null) return;

        // ✅ Horizontal move (2D)
        float moveX = input.MoveInput.x * moveSpeed;
        rb.linearVelocity = new Vector2(moveX, rb.linearVelocity.y);

        // ✅ Jump
        if (input.JumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ✅ Ground detection 2D
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
            
        }
    }
}
