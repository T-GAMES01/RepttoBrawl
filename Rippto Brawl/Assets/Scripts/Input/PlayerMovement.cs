using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private float moveX;
    private bool jumpRequested;

    private int jumpCount = 0;
    private bool isDashing = false;
    private float dashTime;
    private float lastDashTime = -Mathf.Infinity;

    private int facingDirection = 1;
    private Vector3 originalScale;

    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        originalScale = transform.localScale; // ✅ original scale save
    }

    private void FixedUpdate()
    {
        // ✅ DASH
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(facingDirection * dashSpeed, 0);
            dashTime -= Time.fixedDeltaTime;
            if (dashTime <= 0) isDashing = false;
            return;
        }

        // ✅ MOVEMENT
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        // ✅ FACE FLIP
        if (moveX > 0.1f) FaceDirection(1);
        else if (moveX < -0.1f) FaceDirection(-1);

        // ✅ JUMP
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;
            jumpRequested = false;
        }
    }

    private void FaceDirection(int dir)
    {
        if (facingDirection != dir)
        {
            facingDirection = dir;
            Vector3 scale = originalScale; // ✅ always use original scale
            scale.x *= dir;                // flip x-axis only
            transform.localScale = scale;
        }
    }

    public void Move(float horizontal) => moveX = horizontal;

    public void Jump()
    {
        if (jumpCount < 2) jumpRequested = true; // double jump support
    }

    public void Dash()
    {
        if (!isDashing && Time.time - lastDashTime >= dashCooldown)
        {
            isDashing = true;
            dashTime = dashDuration;
            lastDashTime = Time.time;
        }
    }

    public void ResetJump() => jumpCount = 0;
}
