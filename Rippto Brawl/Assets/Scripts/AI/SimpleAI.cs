using UnityEngine;

public class SimpleAI : MonoBehaviour
{
    public float moveDistance = 1.5f;

    public void HandleAI(Transform target, PlayerMovement movement, AttackController attackCtrl)
    {
        if (target == null) return;

        float distance = target.position.x - transform.position.x;

        if (Mathf.Abs(distance) > moveDistance)
        {
            movement.Move(Mathf.Sign(distance));
        }
        else
        {
            movement.Move(0);
            attackCtrl.TryAttack(target);
        }
    }
}
