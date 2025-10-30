using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private InputHandler input;

    private bool isGrounded;
    private int jumpCount = 0;

    private bool isDashing = false;
    private float dashTime;
    private float lastDashTime = -Mathf.Infinity;

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

        var gc = GlobalConstants.Instance; // ✅ FIXED variable name

        // ✅ DASH LOGIC
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(facingDirection * gc.DASH_SPEED, 0);
            dashTime -= Time.fixedDeltaTime;
            if (dashTime <= 0) isDashing = false;
            return;
        }

        // ✅ MOVEMENT
        float moveX = input.MoveInput.x;
        rb.linearVelocity = new Vector2(moveX * gc.MOVE_SPEED, rb.linearVelocity.y);

        // ✅ FACE FLIP LOGIC
        if (moveX > 0.1f)
            FaceDirection(1);
        else if (moveX < -0.1f)
            FaceDirection(-1);

        // ✅ JUMP LOGIC
        if (input.JumpPressed && jumpCount < gc.MAX_JUMPS)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, gc.JUMP_FORCE);
            jumpCount++;
            input.ResetJump();
        }

        // ✅ DASH TRIGGER
        if (input.DashPressed && !isDashing && (Time.time - lastDashTime >= gc.DASH_COOLDOWN))
        {
            isDashing = true;
            dashTime = gc.DASH_DURATION;
            lastDashTime = Time.time;
            gc.Log("⚡ Dash Start!");
        }
    }

    private void FaceDirection(int direction)
    {
        if (facingDirection != direction)
        {
            facingDirection = direction;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            transform.localScale = scale;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == GlobalConstants.Instance.LAYER_GROUND)
        {
            isGrounded = true;
            jumpCount = 0;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == GlobalConstants.Instance.LAYER_GROUND)
            isGrounded = false;
    }
}
