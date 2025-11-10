using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{   
    [Header("Scripts")]
    private CharacterMovementAndOtherMechanics movement;

    [Header("Combat Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;

    [Header("Combat Settings")]
    [SerializeField] private float lightAttackDamage = 5f;
    [SerializeField] private float heavyAttackDamage = 10f;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private float knockbackBase = 5f; // base knockback
    [SerializeField] private float knockbackMultiplier = 0.05f; // extra knockback per % damage

    private int comboCounter = 0;
    private float lastAttackTime = 0f;
    private float comboResetTime = 1f;
    
    [Header("Attack Lunges")]
    private Rigidbody2D rb;
    [SerializeField] private float lightAttackLunge = 2f;
    [SerializeField] private float heavyAttackLunge = 4f;

    public bool LockMovement { get; private set; } = false; // <-- New
    private bool isAttacking = false;
   //ool isAirAttack = !movement.IsGrounded;

    private void Awake()
{
    rb = GetComponent<Rigidbody2D>();
    movement = GetComponent<CharacterMovementAndOtherMechanics>();
}

    private void Update()
    {
        HandleAttackInput();
        ResetComboTimer();
    }

    private void HandleAttackInput()
    {
        // Keyboard Controls
        if (Input.GetKeyDown(KeyCode.J))
            LightAttack();
        if (Input.GetKeyDown(KeyCode.K))
            HeavyAttack();
        if (Input.GetKeyDown(KeyCode.C))
            LightAttack();
        if (Input.GetKeyDown(KeyCode.X))
            HeavyAttack();
        // Mouse Controls
        if (Input.GetMouseButtonDown(0)) // Left Mouse Button
            LightAttack();
        if (Input.GetMouseButtonDown(1)) // Right Mouse Button
            HeavyAttack();
    }

private void LightAttack()
{
    bool isAir = !movement.IsGrounded;

    // Detect if player is moving horizontally
    float horiz = Input.GetAxisRaw("Horizontal");
    bool isMoving = Mathf.Abs(horiz) > 0.1f;

    if (isMoving && !isAir) 
    {
        // Trigger SideKick while moving on ground
        animator.SetTrigger("SideKick");

        // Apply lunge like a light running kick
        float dir = transform.localScale.x > 0 ? 1 : -1;
        float lunge = lightAttackLunge * 1.2f; // slightly longer than normal light
        rb.AddForce(new Vector2(dir * lunge, 0), ForceMode2D.Impulse);

        // Update combo & attack timer
        comboCounter++;
        lastAttackTime = Time.time;

        return; // exit to prevent normal light attack from firing
    }

    // Normal light attack if not moving
    animator.SetTrigger(isAir ? "AirLightAttack" : "LightAttack");

    float dirNormal = transform.localScale.x > 0 ? 1 : -1;
    float lungeNormal = isAir ? lightAttackLunge * 0.5f : lightAttackLunge;
    rb.AddForce(new Vector2(dirNormal * lungeNormal, 0), ForceMode2D.Impulse);

    comboCounter++;
    lastAttackTime = Time.time;
}

private void HeavyAttack()
{
    bool isAir = !movement.IsGrounded;

    float horiz = Input.GetAxisRaw("Horizontal");
    bool movingSideways = Mathf.Abs(horiz) > 0.1f;

    if (movingSideways)
    {
        animator.SetTrigger("FlyingFuChainAttack");

        float dir = transform.localScale.x > 0 ? 1 : -1;
        rb.AddForce(new Vector2(dir * heavyAttackLunge * 1.5f, 0), ForceMode2D.Impulse);

        return;
    }

    animator.SetTrigger(isAir ? "AirHeavyAttack" : "HeavyAttack");

    float dir2 = transform.localScale.x > 0 ? 1 : -1;
    float lunge = isAir ? heavyAttackLunge * 0.6f : heavyAttackLunge;
    rb.AddForce(new Vector2(dir2 * lunge, 0), ForceMode2D.Impulse);
}

    private void ResetComboTimer()
    {
        if (Time.time - lastAttackTime > comboResetTime)
            comboCounter = 0;
    }

    // Called by Animation Events
    public void DealDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            PlayerStats enemyStats = enemy.GetComponent<PlayerStats>();
            if (enemyStats != null)
            {
                // Damage
                float damage = animator.GetCurrentAnimatorStateInfo(0).IsName("HeavyAttack") ? heavyAttackDamage : lightAttackDamage;
                damage *= 1 + (comboCounter * 0.1f); // combo bonus

                enemyStats.TakeDamage(damage);

                // Knockback
                Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 direction = (enemy.transform.position - transform.position).normalized;
                    float knockbackForce = knockbackBase + (enemyStats.DamagePercent * knockbackMultiplier);
                    rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
   /*ublic void UnlockMovement()
{
    LockMovement = false;
    isAttacking = false;
}*/
}
