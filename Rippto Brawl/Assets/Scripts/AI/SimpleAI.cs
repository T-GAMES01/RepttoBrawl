using UnityEngine;

public class SimpleAI : MonoBehaviour
{
    public Transform target;
    private PlayerMovement movement;
    private AttackController attack;
    private float decisionTimer;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        attack = GetComponent<AttackController>();
        decisionTimer = Random.Range(1f, 3f);
    }

    public void UpdateAI()
    {
        if (target == null) return;

        decisionTimer -= Time.deltaTime;
        if (decisionTimer <= 0)
        {
            MakeDecision();
            decisionTimer = Random.Range(1f, 3f);
        }
    }

    void MakeDecision()
    {
        Vector2 dir = target.position - transform.position;

        // Move left/right
        movement.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(Mathf.Sign(dir.x) * GlobalConstants.Instance.MOVE_SPEED, 0);

        // Attack if close
        if (Vector2.Distance(target.position, transform.position) < 2f)
        {
            attack?.DoAttack();
        }
    }
}
