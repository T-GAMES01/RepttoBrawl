using UnityEngine;

/// <summary>
/// CameraMovement
/// Purpose: Brawlhalla-style camera controller that smoothly follows player with zoom, shake, and advanced features.
/// Depends on: Camera component, CharacterMovementAndOtherMechanics (for player reference).
/// Receives: Player position, velocity, jump/dash events.
/// Sends: Smooth camera movement, zoom effects, shake effects.
/// 
/// Features:
/// - Smooth camera follow with configurable speed
/// - Dead zone (camera doesn't move if player is in center)
/// - Look-ahead (camera leads player movement)
/// - Zoom in/out on actions (dash, jump, fall, etc.)
/// - Camera shake on impacts/actions
/// - Camera bounds/limits
/// - Smooth interpolation for all movements
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraMovement : MonoBehaviour
{
    // ============================================
    // TARGET SETTINGS
    // ============================================
    [Header("=== TARGET SETTINGS ===")]
    [Tooltip("The player transform to follow. If null, will try to find player automatically.")]
    [SerializeField] private Transform target;
    
    [Tooltip("Tag name of the player GameObject. Used for auto-finding if target is null.")]
    [SerializeField] private string playerTag = "Player";
    
    [Tooltip("Offset from target position. Camera will follow target + offset.")]
    [SerializeField] private Vector3 targetOffset = Vector3.zero;

    // ============================================
    // FOLLOW SPEED SETTINGS (Brawlhalla-style)
    // ============================================
    [Header("=== FOLLOW SPEED SETTINGS ===")]
    [Tooltip("How fast camera follows player horizontally. Higher = snappier. Brawlhalla uses ~5-8.")]
    [SerializeField] private float followSpeedX = 6.0f;
    
    [Tooltip("How fast camera follows player vertically. Higher = snappier. Brawlhalla uses ~5-8.")]
    [SerializeField] private float followSpeedY = 6.0f;
    
    [Tooltip("Follow speed when player is dashing. Usually faster for action feel.")]
    [SerializeField] private float dashFollowSpeed = 10.0f;
    
    [Tooltip("Follow speed when player is falling. Usually slightly slower for dramatic effect.")]
    [SerializeField] private float fallFollowSpeed = 4.0f;
    
    [Tooltip("Smoothing method: Linear (constant speed) or SmoothDamp (eases in/out).")]
    [SerializeField] private SmoothingMethod smoothingMethod = SmoothingMethod.SmoothDamp;
    
    private enum SmoothingMethod { Linear, SmoothDamp }
    
    [Tooltip("SmoothDamp velocity reference (internal use).")]
    private Vector3 smoothDampVelocity;

    // ============================================
    // DEAD ZONE SETTINGS (Brawlhalla-style)
    // ============================================
    [Header("=== DEAD ZONE SETTINGS ===")]
    [Tooltip("Enable dead zone (camera doesn't move if player is within this area).")]
    [SerializeField] private bool useDeadZone = true;
    
    [Tooltip("Dead zone width (horizontal). Player can move this much without camera moving.")]
    [SerializeField] private float deadZoneWidth = 2.0f;
    
    [Tooltip("Dead zone height (vertical). Player can move this much without camera moving.")]
    [SerializeField] private float deadZoneHeight = 1.5f;
    
    [Tooltip("Dead zone offset from camera center. Adjust to favor upper/lower screen.")]
    [SerializeField] private Vector2 deadZoneOffset = Vector2.zero;

    // ============================================
    // LOOK-AHEAD SETTINGS (Brawlhalla-style)
    // ============================================
    [Header("=== LOOK-AHEAD SETTINGS ===")]
    [Tooltip("Enable look-ahead (camera leads player movement direction).")]
    [SerializeField] private bool useLookAhead = true;
    
    [Tooltip("How far ahead camera looks (in units).")]
    [SerializeField] private float lookAheadDistance = 1.5f;
    
    [Tooltip("How quickly look-ahead responds to direction changes.")]
    [SerializeField] private float lookAheadSpeed = 5.0f;
    
    [Tooltip("Look-ahead only when player is moving fast enough.")]
    [SerializeField] private float lookAheadThreshold = 0.5f;
    
    private Vector3 currentLookAhead;

    // ============================================
    // ZOOM SETTINGS (Brawlhalla-style)
    // ============================================
    [Header("=== ZOOM SETTINGS ===")]
    [Tooltip("Default camera orthographic size (default zoom level).")]
    [SerializeField] private float defaultZoom = 5.0f;
    
    [Tooltip("Minimum zoom (zoomed in).")]
    [SerializeField] private float minZoom = 4.0f;
    
    [Tooltip("Maximum zoom (zoomed out).")]
    [SerializeField] private float maxZoom = 7.0f;
    
    [Tooltip("Zoom speed (how fast camera zooms in/out).")]
    [SerializeField] private float zoomSpeed = 2.0f;
    
    [Tooltip("Zoom in when player dashes.")]
    [SerializeField] private bool zoomOnDash = true;
    
    [Tooltip("Dash zoom amount (how much to zoom in).")]
    [SerializeField] private float dashZoomAmount = 0.5f;
    
    [Tooltip("Zoom in when player jumps.")]
    [SerializeField] private bool zoomOnJump = true;
    
    [Tooltip("Jump zoom amount (how much to zoom in).")]
    [SerializeField] private float zoomOnJumpAmount = 0.3f;
    
    [Tooltip("Zoom out when player falls fast.")]
    [SerializeField] private bool zoomOnFastFall = true;
    
    [Tooltip("Fast fall zoom amount (how much to zoom out).")]
    [SerializeField] private float zoomOnFastFallAmount = 0.4f;
    
    [Tooltip("Zoom out when player moves fast horizontally.")]
    [SerializeField] private bool zoomOnSpeed = true;
    
    [Tooltip("Speed threshold for speed-based zoom.")]
    [SerializeField] private float speedZoomThreshold = 8.0f;
    
    [Tooltip("Maximum zoom out from speed.")]
    [SerializeField] private float speedZoomAmount = 0.5f;
    
    private float targetZoom;
    private float currentZoom;

    // ============================================
    // CAMERA SHAKE SETTINGS (Brawlhalla-style)
    // ============================================
    [Header("=== CAMERA SHAKE SETTINGS ===")]
    [Tooltip("Enable camera shake effects.")]
    [SerializeField] private bool enableShake = true;
    
    [Tooltip("Shake intensity when player lands.")]
    [SerializeField] private float landShakeIntensity = 0.2f;
    
    [Tooltip("Shake duration when player lands.")]
    [SerializeField] private float landShakeDuration = 0.3f;
    
    [Tooltip("Shake intensity when player dashes.")]
    [SerializeField] private float dashShakeIntensity = 0.15f;
    
    [Tooltip("Shake duration when player dashes.")]
    [SerializeField] private float dashShakeDuration = 0.2f;
    
    [Tooltip("Shake intensity when player takes damage/hit.")]
    [SerializeField] private float hitShakeIntensity = 0.4f;
    
    [Tooltip("Shake duration when player takes damage/hit.")]
    [SerializeField] private float hitShakeDuration = 0.4f;
    
    private float shakeTimer;
    private float shakeIntensity;
    private Vector3 shakeOffset;

    // ============================================
    // CAMERA BOUNDS SETTINGS
    // ============================================
    [Header("=== CAMERA BOUNDS SETTINGS ===")]
    [Tooltip("Enable camera bounds (limit camera movement to area).")]
    [SerializeField] private bool useBounds = false;
    
    [Tooltip("Minimum X position camera can reach.")]
    [SerializeField] private float minX = -10f;
    
    [Tooltip("Maximum X position camera can reach.")]
    [SerializeField] private float maxX = 10f;
    
    [Tooltip("Minimum Y position camera can reach.")]
    [SerializeField] private float minY = -10f;
    
    [Tooltip("Maximum Y position camera can reach.")]
    [SerializeField] private float maxY = 10f;

    // ============================================
    // ADVANCED SETTINGS
    // ============================================
    [Header("=== ADVANCED SETTINGS ===")]
    [Tooltip("SmoothDamp time for horizontal movement (lower = faster response).")]
    [SerializeField] private float smoothTimeX = 0.2f;
    
    [Tooltip("SmoothDamp time for vertical movement (lower = faster response).")]
    [SerializeField] private float smoothTimeY = 0.2f;
    
    [Tooltip("Camera Z position (depth). Usually keep at -10 for 2D.")]
    [SerializeField] private float cameraZ = -10f;
    
    [Tooltip("Enable debug visualization (shows dead zone, bounds, etc.).")]
    [SerializeField] private bool showDebugGizmos = false;

    // ============================================
    // PRIVATE VARIABLES
    // ============================================
    private Camera cam;
    private CharacterMovementAndOtherMechanics playerMovement;
    private Rigidbody2D playerRigidbody;
    private Vector3 targetPosition;
    private Vector3 lastTargetPosition;
    private bool wasGrounded;
    private bool wasDashing;
    private bool wasJumping;

    // ============================================
    // UNITY LIFECYCLE
    // ============================================
    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Camera component is required!");
            return;
        }
        
        // Set default zoom
        if (cam.orthographic)
        {
            currentZoom = cam.orthographicSize;
            targetZoom = defaultZoom;
            cam.orthographicSize = defaultZoom;
        }
        
        // Initialize values
        smoothDampVelocity = Vector3.zero;
        currentLookAhead = Vector3.zero;
        shakeOffset = Vector3.zero;
        shakeTimer = 0f;
        targetPosition = transform.position;
        lastTargetPosition = targetPosition;
    }

    private void Start()
    {
        // Find target if not assigned
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
            else
            {
                Debug.LogWarning($"CameraMovement: Could not find player with tag '{playerTag}'. Camera will not follow.");
            }
        }
        
        // Get player movement script for advanced features
        if (target != null)
        {
            playerMovement = target.GetComponent<CharacterMovementAndOtherMechanics>();
            playerRigidbody = target.GetComponent<Rigidbody2D>();
            
            // Initialize camera position to target
            if (target != null)
            {
                transform.position = new Vector3(target.position.x, target.position.y, cameraZ) + targetOffset;
                targetPosition = transform.position;
                lastTargetPosition = targetPosition;
            }
        }
        
        // Initialize state tracking
        if (playerMovement != null)
        {
            wasGrounded = true; // Assume starting on ground
            wasDashing = false;
            wasJumping = false;
        }
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            return;
        }
        
        // Calculate target position
        CalculateTargetPosition();
        
        // Apply camera movement
        ApplyCameraMovement();
        
        // Update zoom
        UpdateZoom();
        
        // Update camera shake
        UpdateShake();
        
        // Apply final position
        ApplyFinalPosition();
        
        // Track state changes for effects
        TrackStateChanges();
    }

    // ============================================
    // TARGET POSITION CALCULATION
    // ============================================
    private void CalculateTargetPosition()
    {
        Vector3 desiredPosition = target.position + targetOffset;
        
        // Apply dead zone
        if (useDeadZone)
        {
            desiredPosition = ApplyDeadZone(desiredPosition);
        }
        
        // Apply look-ahead
        if (useLookAhead && playerRigidbody != null)
        {
            desiredPosition += CalculateLookAhead();
        }
        
        targetPosition = desiredPosition;
    }

    private Vector3 ApplyDeadZone(Vector3 desiredPosition)
    {
        Vector3 cameraPos = transform.position;
        Vector3 delta = desiredPosition - cameraPos;
        
        // Apply dead zone offset
        Vector3 deadZoneCenter = cameraPos + new Vector3(deadZoneOffset.x, deadZoneOffset.y, 0);
        Vector3 relativeDelta = desiredPosition - deadZoneCenter;
        
        // Clamp within dead zone
        if (Mathf.Abs(relativeDelta.x) < deadZoneWidth * 0.5f)
        {
            desiredPosition.x = cameraPos.x;
        }
        
        if (Mathf.Abs(relativeDelta.y) < deadZoneHeight * 0.5f)
        {
            desiredPosition.y = cameraPos.y;
        }
        
        return desiredPosition;
    }

    private Vector3 CalculateLookAhead()
    {
        if (playerRigidbody == null)
        {
            return Vector3.zero;
        }
        
        Vector3 velocity = playerRigidbody.linearVelocity;
        float speed = velocity.magnitude;
        
        // Only look ahead if moving fast enough
        if (speed < lookAheadThreshold)
        {
            currentLookAhead = Vector3.Lerp(currentLookAhead, Vector3.zero, lookAheadSpeed * Time.deltaTime);
            return currentLookAhead;
        }
        
        // Calculate look-ahead direction
        Vector3 lookDirection = velocity.normalized;
        Vector3 targetLookAhead = lookDirection * lookAheadDistance;
        
        // Smoothly interpolate look-ahead
        currentLookAhead = Vector3.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);
        
        return currentLookAhead;
    }

    // ============================================
    // CAMERA MOVEMENT
    // ============================================
    private void ApplyCameraMovement()
    {
        // Determine follow speed based on player state
        float speedX = followSpeedX;
        float speedY = followSpeedY;
        
        if (playerMovement != null && playerRigidbody != null)
        {
            Vector2 velocity = playerRigidbody.linearVelocity;
            float speed = velocity.magnitude;
            
            // Check if player is dashing (high horizontal speed with low vertical)
            bool isDashing = speed > 12f && Mathf.Abs(velocity.y) < 2f && Mathf.Abs(velocity.x) > 10f;
            
            if (isDashing)
            {
                speedX = dashFollowSpeed;
                speedY = dashFollowSpeed;
            }
            // Check if player is falling fast
            else if (velocity.y < -5f)
            {
                speedY = fallFollowSpeed;
            }
        }
        
        // Apply smoothing
        Vector3 currentPos = transform.position;
        Vector3 newPosition;
        
        if (smoothingMethod == SmoothingMethod.SmoothDamp)
        {
            // Use SmoothDamp for smooth easing
            float smoothX = Mathf.SmoothDamp(currentPos.x, targetPosition.x, ref smoothDampVelocity.x, smoothTimeX, Mathf.Infinity, Time.deltaTime);
            float smoothY = Mathf.SmoothDamp(currentPos.y, targetPosition.y, ref smoothDampVelocity.y, smoothTimeY, Mathf.Infinity, Time.deltaTime);
            newPosition = new Vector3(smoothX, smoothY, cameraZ);
        }
        else
        {
            // Use linear interpolation
            float moveX = Mathf.Lerp(currentPos.x, targetPosition.x, speedX * Time.deltaTime);
            float moveY = Mathf.Lerp(currentPos.y, targetPosition.y, speedY * Time.deltaTime);
            newPosition = new Vector3(moveX, moveY, cameraZ);
        }
        
        // Apply bounds
        if (useBounds)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        }
        
        transform.position = newPosition;
    }

    // ============================================
    // ZOOM MANAGEMENT
    // ============================================
    private void UpdateZoom()
    {
        if (!cam.orthographic)
        {
            return; // Zoom only works with orthographic cameras
        }
        
        // Start with default zoom
        targetZoom = defaultZoom;
        
        // Apply zoom modifiers
        if (playerRigidbody != null)
        {
            float speed = playerRigidbody.linearVelocity.magnitude;
            Vector2 velocity = playerRigidbody.linearVelocity;
            
            // Check if grounded (for jump zoom)
            bool isGrounded = Physics2D.Raycast(target.position, Vector2.down, 0.3f);
            
            // Zoom on dash (check current dash state)
            bool isDashing = speed > 12f && Mathf.Abs(velocity.y) < 2f && Mathf.Abs(velocity.x) > 10f;
            if (zoomOnDash && isDashing)
            {
                targetZoom -= dashZoomAmount;
            }
            
            // Zoom on jump (check current jump state)
            bool isJumping = velocity.y > 0.1f && !isGrounded;
            if (zoomOnJump && isJumping)
            {
                targetZoom -= zoomOnJumpAmount;
            }
            
            // Zoom on fast fall
            if (zoomOnFastFall && velocity.y < -8f)
            {
                targetZoom += zoomOnFastFallAmount;
            }
            
            // Zoom on speed
            if (zoomOnSpeed && speed > speedZoomThreshold)
            {
                float speedFactor = (speed - speedZoomThreshold) / 10f; // Normalize
                targetZoom += Mathf.Clamp(speedFactor * speedZoomAmount, 0f, speedZoomAmount);
            }
        }
        
        // Clamp zoom
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        
        // Smoothly interpolate zoom
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, zoomSpeed * Time.deltaTime);
        cam.orthographicSize = currentZoom;
    }

    // ============================================
    // CAMERA SHAKE
    // ============================================
    private void UpdateShake()
    {
        if (!enableShake)
        {
            shakeOffset = Vector3.zero;
            return;
        }
        
        if (shakeTimer > 0f)
        {
            // Generate random shake offset
            shakeOffset = Random.insideUnitCircle * shakeIntensity;
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            // Fade out shake
            shakeOffset = Vector3.Lerp(shakeOffset, Vector3.zero, 10f * Time.deltaTime);
        }
    }

    // Public method to trigger shake (can be called from other scripts)
    public void TriggerShake(float intensity, float duration)
    {
        if (!enableShake)
        {
            return;
        }
        
        shakeIntensity = intensity;
        shakeTimer = duration;
    }

    // Convenience methods for common shake types
    public void TriggerLandShake()
    {
        TriggerShake(landShakeIntensity, landShakeDuration);
    }

    public void TriggerDashShake()
    {
        TriggerShake(dashShakeIntensity, dashShakeDuration);
    }

    public void TriggerHitShake()
    {
        TriggerShake(hitShakeIntensity, hitShakeDuration);
    }

    // ============================================
    // STATE TRACKING
    // ============================================
    private void TrackStateChanges()
    {
        if (playerMovement == null || playerRigidbody == null)
        {
            return;
        }
        
        Vector2 velocity = playerRigidbody.linearVelocity;
        float speed = velocity.magnitude;
        
        // Track landing (for shake effect) - use ground check
        bool isGrounded = Physics2D.Raycast(target.position, Vector2.down, 0.3f);
        if (!wasGrounded && isGrounded && velocity.y <= 0.5f)
        {
            // Player just landed - trigger landing shake
            TriggerLandShake();
        }
        wasGrounded = isGrounded;
        
        // Track dash state (high horizontal speed with low vertical movement)
        bool isDashing = speed > 12f && Mathf.Abs(velocity.y) < 2f && Mathf.Abs(velocity.x) > 10f;
        
        if (!wasDashing && isDashing)
        {
            // Player just dashed - trigger dash shake
            TriggerDashShake();
        }
        wasDashing = isDashing;
        
        // Track jump state (moving upward)
        bool isJumping = velocity.y > 0.1f && !isGrounded;
        wasJumping = isJumping;
    }

    // ============================================
    // FINAL POSITION APPLICATION
    // ============================================
    private void ApplyFinalPosition()
    {
        // Apply shake offset to final position
        Vector3 finalPosition = transform.position + shakeOffset;
        finalPosition.z = cameraZ; // Ensure Z stays constant
        transform.position = finalPosition;
        
        lastTargetPosition = targetPosition;
    }

    // ============================================
    // DEBUG VISUALIZATION
    // ============================================
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos)
        {
            return;
        }
        
        // Draw dead zone
        if (useDeadZone)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = transform.position + new Vector3(deadZoneOffset.x, deadZoneOffset.y, 0);
            Gizmos.DrawWireCube(center, new Vector3(deadZoneWidth, deadZoneHeight, 0));
        }
        
        // Draw camera bounds
        if (useBounds)
        {
            Gizmos.color = Color.red;
            Vector3 min = new Vector3(minX, minY, 0);
            Vector3 max = new Vector3(maxX, maxY, 0);
            Vector3 center = (min + max) * 0.5f;
            Vector3 size = max - min;
            Gizmos.DrawWireCube(center, size);
        }
        
        // Draw look-ahead
        if (useLookAhead && target != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 targetPos = target.position;
            Vector3 lookAheadPos = targetPos + currentLookAhead;
            Gizmos.DrawLine(targetPos, lookAheadPos);
            Gizmos.DrawWireSphere(lookAheadPos, 0.2f);
        }
    }
}

