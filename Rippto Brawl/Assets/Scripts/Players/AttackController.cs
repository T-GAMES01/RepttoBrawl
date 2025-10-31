using UnityEngine;

public class AttackController : MonoBehaviour
{
    public int damage = 10;
    public float attackRange = 1.2f;
    public float attackCooldown = 0.5f;
    private bool canAttack = true;

    public void TryAttack(Transform target)
    {
        if (target == null || !canAttack) return;

        float dist = Vector2.Distance(transform.position, target.position);
        if (dist <= attackRange)
        {
            PlayerHealth enemy = target.GetComponent<PlayerHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, transform.position);
            }
            canAttack = false;
            Invoke(nameof(ResetAttack), attackCooldown);
        }
    }

    private void ResetAttack() => canAttack = true;
}
