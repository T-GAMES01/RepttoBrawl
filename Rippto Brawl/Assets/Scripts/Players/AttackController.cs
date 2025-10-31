using UnityEngine;

public class AttackController : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackDamage = 10f;
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;
    public Transform attackPoint;

    public void DoAttack()
    {
        if (attackPoint == null)
        {
            Debug.LogWarning("Attack point missing!");
            return;
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (var enemy in hitEnemies)
        {
            Vector2 dir = (enemy.transform.position - transform.position).normalized;
            enemy.GetComponent<PlayerHealth>()?.TakeHit(dir, attackDamage);
        }

        GlobalConstants.Instance.Log("⚔️ Attack triggered!");
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
