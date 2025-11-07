using UnityEngine;

/// <summary>
/// CharacterMovementAndOtherMechanics
/// Purpose: Brawlhalla-style character controller with smooth, responsive movement mechanics.
/// Depends on: Rigidbody2D, GroundCheck (Collider2D or LayerMask), Input system (Horizontal, Jump).
/// Receives: Input from player (horizontal movement, jump, dash, fast fall).
/// Sends: Physics-based movement via Rigidbody2D, visual feedback through animations.
/// 
/// Features:
/// - Exact Brawlhalla gravity, movement speed, and jump mechanics
/// - Coyote time for forgiving edge jumps
/// - Jump buffering for responsive feel
/// - Fixed jump height (consistent height, not affected by button hold)
/// - Fast fall mechanics
/// - Dash system with cooldown
/// - Smooth acceleration/deceleration for responsive controls
/// - Complete animation system: Run, Jump (ground + air), Fall, Dash animations
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class CharacterMovementAndOtherMechanics : MonoBehaviour
{
    // ============================================
    // GRAVITY SETTINGS (Brawlhalla: -25.0)
    // ============================================
    [Header("=== GRAVITY SETTINGS ===")]
    [Tooltip("Gravity strength when falling. Brawlhalla uses -25.0 for quick, snappy falls.")]
    [SerializeField] private float gravity = -25.0f;
    
    [Tooltip("Additional gravity multiplier when fast falling. Makes character fall faster.")]
    [SerializeField] private float fastFallGravityMultiplier = 1.5f;
    
    [Tooltip("Gravity multiplier when moving upward (reduces upward momentum). Lower values = floatier jumps.")]
    [SerializeField] private float upwardGravityMultiplier = 0.8f;

    // ============================================
    // MOVEMENT SPEED SETTINGS (Brawlhalla: 7.0)
    // ============================================
    [Header("=== MOVEMENT SPEED SETTINGS ===")]
    [Tooltip("Maximum horizontal movement speed on ground. Brawlhalla uses ~7.0 units/second.")]
    [SerializeField] private float groundMoveSpeed = 7.0f;
    
    [Tooltip("Maximum horizontal movement speed in air. Typically 60-80% of ground speed.")]
    [SerializeField] private float airMoveSpeed = 5.0f;
    
    [Tooltip("How quickly character accelerates to max speed. Higher = snappier response.")]
    [SerializeField] private float acceleration = 50.0f;
    
    [Tooltip("How quickly character decelerates when stopping. Higher = more responsive stops.")]
    [SerializeField] private float deceleration = 60.0f;
    
    [Tooltip("Friction multiplier when grounded. 1.0 = no friction, higher = more friction.")]
    [SerializeField] private float groundFriction = 0.9f;

    // ============================================
    // JUMP HEIGHT & SPEED SETTINGS (Brawlhalla: ~2.0 height)
    // ============================================
    [Header("=== JUMP HEIGHT & SPEED SETTINGS ===")]
    [Tooltip("Desired jump height in units. Brawlhalla characters jump ~2.0 units high.")]
    [SerializeField] private float jumpHeight = 2.0f;
    
    [Tooltip("Initial jump velocity. Calculated from jumpHeight and gravity for exact physics.")]
    [SerializeField] private float jumpForce = 10.0f;
    
    [Tooltip("Maximum number of jumps (including ground jump). Brawlhalla uses 3 total jumps (1 ground + 2 air). After 3 jumps, character must touch ground to reset.")]
    [SerializeField] private int maxJumps = 3;
    
    [Tooltip("Multiplier for air jumps. 1.0 = same as ground jump, <1.0 = weaker air jumps.")]
    [SerializeField] private float airJumpMultiplier = 0.9f;
    
    [Tooltip("NOTE: Variable jump height is disabled. Jump height is fixed regardless of button hold duration.")]
    [SerializeField] private bool variableJumpDisabled = true;

    // ============================================
    // PLAYER PSYCHOLOGY FEATURES (Smooth & Forgiving)
    // ============================================
    [Header("=== PLAYER PSYCHOLOGY FEATURES ===")]
    [Tooltip("Coyote Time: Time window after leaving ground where you can still jump. Makes edge jumps feel forgiving.")]
    [SerializeField] private float coyoteTime = 0.15f;
    
    [Tooltip("Jump Buffer: Time window before landing where jump input is remembered. Prevents missed jumps.")]
    [SerializeField] private float jumpBufferTime = 0.1f;
    
    [Tooltip("Time window after landing where character can jump. Helps with quick successive jumps.")]
    [SerializeField] private float landingJumpGraceTime = 0.05f;

    // ============================================
    // DASH MECHANICS (Brawlhalla-style)
    // ============================================
    [Header("=== DASH MECHANICS ===")]
    [Tooltip("Horizontal dash speed. Should be significantly faster than ground move speed.")]
    [SerializeField] private float dashSpeed = 14.0f;
    
    [Tooltip("Duration of dash in seconds. Brawlhalla dashes are quick (~0.2s).")]
    [SerializeField] private float dashDuration = 0.2f;
    
    [Tooltip("Cooldown between dashes. Prevents dash spamming.")]
    [SerializeField] private float dashCooldown = 0.5f;
    
    [Tooltip("Number of dashes available. Brawlhalla typically has 1-2 dashes.")]
    [SerializeField] private int maxDashes = 1;

    // ============================================
    // FAST FALL SETTINGS
    // ============================================
    [Header("=== FAST FALL SETTINGS ===")]
    [Tooltip("Fast fall speed multiplier. Increases downward velocity when fast falling.")]
    [SerializeField] private float fastFallSpeedMultiplier = 2.0f;
    
    [Tooltip("Minimum downward velocity to trigger fast fall visual effects.")]
    [SerializeField] private float fastFallThreshold = -8.0f;

    // ============================================
    // GROUND DETECTION SETTINGS
    // ============================================
    [Header("=== GROUND DETECTION SETTINGS ===")]
    [Tooltip("Distance to check for ground below character. Should match character's feet position.")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    
    [Tooltip("Layer mask for ground objects. Only objects on these layers count as ground.")]
    [SerializeField] private LayerMask groundLayerMask = 1;
    
    [Tooltip("Width of ground check box. Wider = more forgiving ground detection.")]
    [SerializeField] private float groundCheckWidth = 0.5f;

    // ============================================
    // ANIMATION SETTINGS
    // ============================================
    [Header("=== ANIMATION SETTINGS ===")]
    [Tooltip("Animator component reference. Will be auto-assigned if not set.")]
    [SerializeField] private Animator animator;
    
    [Tooltip("Minimum horizontal speed to trigger run animation.")]
    [SerializeField] private float runAnimationThreshold = 0.1f;
    
    [Tooltip("Minimum downward velocity to trigger fall animation.")]
    [SerializeField] private float fallAnimationThreshold = -0.5f;
    
    [Tooltip("Animation parameter names (can be customized in Inspector if your Animator uses different names)")]
    [SerializeField] private string animParamIsGrounded = "IsGrounded";
    [SerializeField] private string animParamIsRunning = "IsRunning";
    [SerializeField] private string animParamVerticalVelocity = "VerticalVelocity";
    [SerializeField] private string animParamHorizontalSpeed = "HorizontalSpeed";
    [SerializeField] private string animParamJumpTrigger = "JumpTrigger";
    [SerializeField] private string animParamCurrentJumpNumber = "CurrentJumpNumber";
    [SerializeField] private string animParamIsFalling = "IsFalling";
    [SerializeField] private string animParamIsDashing = "IsDashing";
    
    [Tooltip("Flip character sprite based on movement direction. Set to false if you handle flipping in Animator.")]
    [SerializeField] private bool flipSpriteOnMovement = true;

    // ============================================
    // PRIVATE VARIABLES
    // ============================================
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float landingGraceCounter;
    private int currentJumps;
    private int currentDashes;
    private float dashTimer;
    private float dashCooldownTimer;
    private bool isDashing;
    private bool isFastFalling;
    private float horizontalInput;
    private Vector2 dashDirection;
    
    // Animation state tracking
    private bool wasJumping;
    private bool justJumped;
    private float lastVerticalVelocity;
    
    //>>>>>>>>>>> DUST_RUNNING <<<<<<<<<<<<<//
    [SerializeField] private DustRunning dustRun;
    // ============================================
    // UNITY LIFECYCLE
    // ============================================
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component is required!");
            return;
        }
        
        // Check for Collider2D
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("Collider2D component is required for ground detection!");
            return;
        }
        
        // Get Animator component
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("Animator component not found! Animations will not work.");
            }
        }
        
        // Configure Rigidbody2D for platform fighter feel
        rb.gravityScale = 0; // We handle gravity manually for precise control
        rb.freezeRotation = true; // Prevent character from rotating
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Smooth collision detection
        rb.bodyType = RigidbodyType2D.Dynamic; // Ensure it's dynamic, not kinematic
        
        // Calculate jump force from jump height for exact physics
        // Using physics formula: v = sqrt(2 * g * h) where g is positive gravity
        jumpForce = Mathf.Sqrt(2 * Mathf.Abs(gravity) * jumpHeight);
        
        // Initialize animation state
        wasJumping = false;
        justJumped = false;
        lastVerticalVelocity = 0f;
    }

    private void Start()
    {
        currentJumps = 0;
        currentDashes = maxDashes;
        isDashing = false;
        isFastFalling = false;
    }

    private void Update()
    {
       // transform.localScale = new Vector3(-1, 1, 1);   // force right
        // Get input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        // Jump only triggers on key press (GetButtonDown), not while holding
        
        // Update timers
        UpdateTimers();
        
        // Check ground state
        CheckGrounded();
        
        // Handle jump input - ONLY on key press, not while holding
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        
        // Handle dash input (using LeftShift by default, or "Dash" button if configured in Input Manager)
        // Note: "Dash" button must be configured in Edit -> Project Settings -> Input Manager if you want to use it
        bool dashInput = Input.GetKeyDown(KeyCode.LeftShift);
        
        // Try to use "Dash" button if it exists (non-blocking check)
        try
        {
            dashInput = dashInput || Input.GetButtonDown("Dash");
        }
        catch (System.ArgumentException)
        {
            // "Dash" button not configured - that's okay, we'll use LeftShift
        }
        
        if (dashInput)
        {
            TryDash();
        }
        
        // Handle fast fall input
        if ((Input.GetAxisRaw("Vertical") < -0.5f || Input.GetKey(KeyCode.S)) && !isGrounded && rb.linearVelocity.y < 0)
        {
            isFastFalling = true;
        }
        else
        {
            isFastFalling = false;
        }
        
        // Update animations
        UpdateAnimations();
        
        // Flip sprite based on movement direction
       /* if (flipSpriteOnMovement)
        {
            FlipSpriteBasedOnMovement();
        }*/
        // --- FORCE FLIP (simple & reliable) ---  
if (horizontalInput > 0f)        // D key → face right
{
    transform.localScale = new Vector3(-0.25f, transform.localScale.y, transform.localScale.z);
}
else if (horizontalInput < 0f)   // A key → face left
{
    transform.localScale = new Vector3(0.25f, transform.localScale.y, transform.localScale.z);
}
//////
    bool isMoving = horizontalInput != 0;
bool playRunDust = isMoving && isGrounded;

if (dustRun != null)
{
    dustRun.SetRunningDust(playRunDust);
}

        
    }

    private void FixedUpdate()
    {
        // Apply gravity
        ApplyGravity();
        
        // Handle movement (only if not dashing)
        if (!isDashing)
        {
            HandleMovement();
        }
        
        // Handle jumping
        HandleJump();
        
        // Handle fast fall
        if (isFastFalling)
        {
            ApplyFastFall();
        }
        
        // Update dash state
        UpdateDash();
    }

    // ============================================
    // MOVEMENT METHODS
    // ============================================
    private void HandleMovement()
    {
        float targetSpeed = isGrounded ? groundMoveSpeed : airMoveSpeed;
        
        // Calculate target velocity based on input
        float targetVelocity = horizontalInput * targetSpeed;
        
        // Get current horizontal velocity
        float currentVelocity = rb.linearVelocity.x;
        
        // Apply acceleration or deceleration
        float velocityChange;
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            // Accelerate towards target speed
            velocityChange = acceleration * Time.fixedDeltaTime;
        }
        else
        {
            // Decelerate when no input
            velocityChange = deceleration * Time.fixedDeltaTime;
            
            // Apply ground friction
            if (isGrounded)
            {
                currentVelocity *= groundFriction;
            }
        }
        
        // Smoothly interpolate velocity
        float newVelocity = Mathf.MoveTowards(currentVelocity, targetVelocity, velocityChange);
        
        // Apply velocity (preserve Y velocity)
        rb.linearVelocity = new Vector2(newVelocity, rb.linearVelocity.y);
    }

    // ============================================
    // JUMP METHODS
    // ============================================
    private void HandleJump()
    {
        // Reset jump count when grounded (Brawlhalla: Must touch ground to reset jump count)
        // After 3 jumps, character MUST touch ground before jumping again
        if (isGrounded)
        {
            currentJumps = 0;
            landingGraceCounter = landingJumpGraceTime;
        }
        
        // Check if we can jump (max 3 jumps total: 1 ground + 2 air)
        // First jump (currentJumps == 0): Must be grounded or in coyote/landing grace window
        // Subsequent jumps (currentJumps > 0): Can jump in air as long as currentJumps < maxJumps (3)
        bool canJump = false;
        
        if (currentJumps == 0)
        {
            // First jump: Must be on ground or have coyote time/landing grace
            // Safety fallbacks for edge cases:
            // 1. Character is at rest and near ground
            // 2. Character is falling (velocity downward) and near ground (just left ground)
            bool isAtRest = Mathf.Abs(rb.linearVelocity.y) < 0.5f;
            bool isFalling = rb.linearVelocity.y < 0.1f;
            bool nearGround = false;
            
            // Quick ground check as safety fallback
            if (isAtRest || isFalling)
            {
                RaycastHit2D safetyCheck = Physics2D.Raycast(
                    transform.position,
                    Vector2.down,
                    groundCheckDistance + 0.5f,
                    groundLayerMask
                );
                nearGround = safetyCheck.collider != null;
            }
            
            // Allow jump if: grounded, coyote time, landing grace, or safety fallback conditions
            canJump = (isGrounded || coyoteTimeCounter > 0 || landingGraceCounter > 0 || 
                      (isAtRest && nearGround) || (isFalling && nearGround && coyoteTimeCounter <= 0));
        }
        else
        {
            // Air jumps: Can jump if we haven't reached max jumps (3 total)
            canJump = currentJumps < maxJumps;
        }
        
        bool shouldJump = jumpBufferCounter > 0 && canJump;
        
        if (shouldJump)
        {
            // Calculate jump force (reduce for air jumps)
            float jumpPower = jumpForce;
            if (currentJumps > 0)
            {
                jumpPower *= airJumpMultiplier;
            }
            
            // Apply jump
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            currentJumps++;
            
            // Mark that we just jumped (for animation trigger)
            justJumped = true;
            
            // Reset counters
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
            landingGraceCounter = 0;
        }
        
        // Fixed jump height: Jump height is consistent, not affected by button hold duration
        // (Variable jump height removed per user request)
    }

    // ============================================
    // DASH METHODS
    // ============================================
    private void TryDash()
    {
        if (currentDashes > 0 && dashCooldownTimer <= 0 && !isDashing)
        {
            // Determine dash direction (prefer input direction, otherwise facing direction)
            float dashInput = horizontalInput;
            if (Mathf.Abs(dashInput) < 0.1f)
            {
                dashInput = transform.localScale.x > 0 ? 1 : -1;
            }
            
            dashDirection = new Vector2(Mathf.Sign(dashInput), 0);
            
            // Start dash
            isDashing = true;
            dashTimer = dashDuration;
            currentDashes--;
            dashCooldownTimer = dashCooldown;
            
            // Apply dash velocity
            rb.linearVelocity = new Vector2(dashDirection.x * dashSpeed, 0);
        }
    }

    private void UpdateDash()
    {
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            
            // Maintain dash velocity during dash
            rb.linearVelocity = new Vector2(dashDirection.x * dashSpeed, rb.linearVelocity.y);
            
            if (dashTimer <= 0)
            {
                isDashing = false;
            }
        }
        
        // Recharge dashes when grounded
        if (isGrounded && currentDashes < maxDashes)
        {
            currentDashes = maxDashes;
        }
        
        // Update cooldown
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.fixedDeltaTime;
        }
    }

    // ============================================
    // GRAVITY & PHYSICS METHODS
    // ============================================
    private void ApplyGravity()
    {
        float currentGravity = gravity;
        
        // Modify gravity when moving upward (for smoother arc)
        // Note: No longer dependent on button hold since variable jump is disabled
        if (rb.linearVelocity.y > 0)
        {
            // Apply upward gravity multiplier for smoother jump arc
            currentGravity *= upwardGravityMultiplier;
        }
        
        // Apply gravity
        rb.linearVelocity += Vector2.up * currentGravity * Time.fixedDeltaTime;
    }

    private void ApplyFastFall()
    {
        // Apply additional downward force for fast fall
        float fastFallForce = (gravity * fastFallGravityMultiplier) - gravity;
        rb.linearVelocity += Vector2.up * fastFallForce * Time.fixedDeltaTime;
        
        // Cap maximum fall speed to prevent excessive speeds
        if (rb.linearVelocity.y < fastFallThreshold)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fastFallThreshold);
        }
    }

    // ============================================
    // GROUND DETECTION METHODS
    // ============================================
    private void CheckGrounded()
    {
        wasGrounded = isGrounded;
        
        // Method 1: Direct layer contact check using attached colliders
        // Fast path: if any collider is touching the ground layers, we are grounded
        isGrounded = false;
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            if (col != null)
            {
                if (col.IsTouchingLayers(groundLayerMask))
                {
                    isGrounded = true;
                    break;
                }
            }
        }

        // Method 2: Check if Rigidbody2D is touching ground using contact normals (bottom-only)
        if (!isGrounded)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(groundLayerMask);
            filter.useTriggers = false;

            foreach (Collider2D col in colliders)
            {
                if (col == null) continue;
                ContactPoint2D[] contacts = new ContactPoint2D[10];
                int contactCount = col.GetContacts(filter, contacts);
                for (int i = 0; i < contactCount; i++)
                {
                    if (contacts[i].normal.y > 0.5f)
                    {
                        isGrounded = true;
                        break;
                    }
                }
                if (isGrounded) break;
            }
        }
        
        // Method 3: Fallback to BoxCast if previous methods didn't detect ground
        // This helps when character is just touching ground or falling slowly
        if (!isGrounded)
        {
            Vector2 boxSize = new Vector2(groundCheckWidth, 0.1f);
            Vector2 boxCenter = (Vector2)transform.position + Vector2.down * (groundCheckDistance * 0.25f);
            
            RaycastHit2D hit = Physics2D.BoxCast(
                boxCenter,
                boxSize,
                0f,
                Vector2.down,
                groundCheckDistance,
                groundLayerMask
            );
            
            if (hit.collider != null)
            {
                isGrounded = true;
            }
        }
        
        // Method 4: Small circle cast at feet (robust against sloped/uneven ground)
        if (!isGrounded)
        {
            float radius = Mathf.Max(0.05f, groundCheckWidth * 0.25f);
            Vector2 feet = (Vector2)transform.position + Vector2.down * (groundCheckDistance * 0.6f);
            RaycastHit2D circle = Physics2D.CircleCast(feet, radius, Vector2.down, 0.02f, groundLayerMask);
            if (circle.collider != null)
            {
                isGrounded = true;
            }
        }

        // Additional check: If velocity is very small and we're moving down, we might be on ground
        // This helps with edge cases where physics hasn't updated yet
        if (!isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.1f && rb.linearVelocity.y <= 0)
        {
            RaycastHit2D quickCheck = Physics2D.Raycast(
                transform.position,
                Vector2.down,
                groundCheckDistance + 0.2f,
                groundLayerMask
            );
            
            if (quickCheck.collider != null)
            {
                isGrounded = true;
            }
        }
        if (wasGrounded && !isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.15f)
{
    isGrounded = true;
}

        // Update coyote time counter
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else if (wasGrounded)
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    // ============================================
    // UTILITY METHODS
    // ============================================
    private void UpdateTimers()
    {
        // Update jump buffer
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
        
        // Update landing grace time
        if (landingGraceCounter > 0)
        {
            landingGraceCounter -= Time.deltaTime;
        }
    }

    // ============================================
    // ANIMATION METHODS
    // ============================================
    private void UpdateAnimations()
    {
        // Return early if animator is not available
        if (animator == null)
        {
            return;
        }

        // Get current velocities
        float currentVerticalVelocity = rb.linearVelocity.y;
        float currentHorizontalSpeed = Mathf.Abs(rb.linearVelocity.x);
        
        // Determine animation states with priority system (mutually exclusive)
        // Priority order: Dash > Jump > Fall > Run > Idle
        
        // 1. DASH - Highest priority: When dashing, nothing else should be true
        bool isDashingState = isDashing;
        
        // 2. JUMP - When jumping (moving upward), running and falling should be false
        bool isJumpingState = !isDashingState && !isGrounded && currentVerticalVelocity > 0.1f;
        
        // 3. FALL - When falling (moving downward in air), running and jumping should be false
        bool isFallingState = !isDashingState && !isGrounded && !isJumpingState && currentVerticalVelocity < fallAnimationThreshold;
        
        // 4. RUN - When running (moving horizontally on ground), jumping and falling should be false
bool isRunningState =
    horizontalInput != 0     // key pressed → RUN/WALK should win
    && isGrounded
    && !isDashingState;


        
        // Reset all animation bools to false first (ensures clean state)
        SetAnimatorBool(animParamIsRunning, false);
        SetAnimatorBool(animParamIsFalling, false);
        SetAnimatorBool(animParamIsDashing, false);
        
        // Now set the active state to true (only one can be true at a time)
        if (isDashingState)
        {
            // DASHING: Only dash is true, all others are false
            SetAnimatorBool(animParamIsDashing, true);
        }
        else if (isJumpingState)
        {
            // JUMPING: Only jump-related animations, running and falling are false
            // (Jump is handled by trigger, but we ensure other states are false)
        }
        else if (isFallingState)
        {
            // FALLING: Only fall is true, running and jumping are false
            SetAnimatorBool(animParamIsFalling, true);
        }
        else if (isRunningState)
        {
            // RUNNING: Only run is true, jumping and falling are false
            SetAnimatorBool(animParamIsRunning, true);
        }
        // If none of the above, character is idle (all bools are false)
        
        // Update IsGrounded (always set, regardless of other states)
        SetAnimatorBool(animParamIsGrounded, isGrounded);
        
        // Update float parameters (these are always set, not mutually exclusive)
        SetAnimatorFloat(animParamVerticalVelocity, currentVerticalVelocity);
        SetAnimatorFloat(animParamHorizontalSpeed, currentHorizontalSpeed);
        
        // CurrentJumpNumber (int) - which jump this is (1, 2, or 3)
        SetAnimatorInt(animParamCurrentJumpNumber, currentJumps);
        
        // JumpTrigger - trigger jump animation when jump occurs
        // This trigger fires independently and the Animator should handle transitioning to jump state
        if (justJumped)
        {
            SetAnimatorTrigger(animParamJumpTrigger);
            justJumped = false; // Reset trigger flag
            
            // Ensure running and falling are false when jump triggers
            SetAnimatorBool(animParamIsRunning, false);
            SetAnimatorBool(animParamIsFalling, false);
        }
        
        // Update tracking variables
        wasJumping = isJumpingState;
        lastVerticalVelocity = currentVerticalVelocity;
    }

    // Helper methods to safely set animator parameters (only if they exist)
    private void SetAnimatorBool(string paramName, bool value)
    {
        if (animator != null && HasParameter(paramName))
        {
            animator.SetBool(paramName, value);
        }
    }

    private void SetAnimatorFloat(string paramName, float value)
    {
        if (animator != null && HasParameter(paramName))
        {
            animator.SetFloat(paramName, value);
        }
    }

    private void SetAnimatorInt(string paramName, int value)
    {
        if (animator != null && HasParameter(paramName))
        {
            animator.SetInteger(paramName, value);
        }
    }

    private void SetAnimatorTrigger(string paramName)
    {
        if (animator != null && HasParameter(paramName))
        {
            animator.SetTrigger(paramName);
        }
    }

    // Check if animator has a parameter with the given name
    private bool HasParameter(string paramName)
    {
        if (animator == null || animator.parameters == null)
        {
            return false;
        }

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }
        return false;
    }

    // ============================================
    // SPRITE FLIPPING
    // ============================================
/*private void FlipSpriteBasedOnMovement()
{
    if (horizontalInput > 0)  // D
    {
        transform.localScale = new Vector3(-0.25f, transform.localScale.y, transform.localScale.z);
    }
    else if (horizontalInput < 0)  // A
    {
        transform.localScale = new Vector3(0.25f, transform.localScale.y, transform.localScale.z);
    }
}*/

    // ============================================
    // DEBUG VISUALIZATION (Optional)
    // ============================================
    private void OnDrawGizmosSelected()
    {
        // Draw ground check area
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector2 origin = (Vector2)transform.position + Vector2.down * groundCheckDistance;
        Vector2 size = new Vector2(groundCheckWidth, 0.1f);
        Gizmos.DrawWireCube(origin, size);
    }
}
