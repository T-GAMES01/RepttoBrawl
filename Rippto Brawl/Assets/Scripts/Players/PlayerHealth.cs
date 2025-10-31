using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float knockbackForce = 8f;
    public bool IsDead { get; private set; }

    private float currentHealth;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int dmg, Vector2 attackerPos)
    {
        if (IsDead) return;

        currentHealth -= dmg;
        float dir = Mathf.Sign(transform.position.x - attackerPos.x);
        rb.AddForce(new Vector2(dir * knockbackForce, knockbackForce), ForceMode2D.Impulse);

        Debug.Log($"{gameObject.name} took {dmg} damage | Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            IsDead = true;
            Debug.Log($"{gameObject.name} KO!");
            Destroy(gameObject, 1f);
        }
    }
}
