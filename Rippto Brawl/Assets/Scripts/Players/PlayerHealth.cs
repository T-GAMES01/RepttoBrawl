using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f; // max limit (sirf info)
    public float currentHealth;    // abhi ki health (Brawlhalla style)

    [Header("Knockback Settings")]
    public float knockbackMultiplier = 5f; // health ke sath knockback barhta ha
    private Rigidbody2D rb;

    [Header("Death Zone Limits")]
    public float deathY = -10f;   // niche girne par death
    public float deathXLeft = -15f;  // left side limit
    public float deathXRight = 15f;  // right side limit

    [Header("Respawn Point")]
    public Vector3 respawnPoint = Vector3.zero; // jahan respawn hoga

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = 0f; // Brawlhalla style (jitni zyada health, utna danger)
    }

    private void Update()
    {
        // agar player screen ke bahar gaya to die
        if (transform.position.y < deathY ||
            transform.position.x < deathXLeft ||
            transform.position.x > deathXRight)
        {
            Die();
        }
    }

    public void TakeHit(Vector2 hitDirection, float damage)
    {
        currentHealth += damage; // damage me health barhti ha (Brawlhalla logic)
        Debug.Log("💢 Health: " + currentHealth);

        // knockback zyada jab health zyada
        float knockForce = knockbackMultiplier * (currentHealth / 10f);
        rb.AddForce(hitDirection * knockForce, ForceMode2D.Impulse);
    }

    private void Die()
    {
        Debug.Log("💀 Player Dead!");
        currentHealth = 0f;
        transform.position = respawnPoint; // respawn hota ha
        rb.linearVelocity = Vector2.zero;
    }
}
