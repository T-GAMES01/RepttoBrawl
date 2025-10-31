using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(AttackController))]
[RequireComponent(typeof(PlayerHealth))]
public class FighterController : MonoBehaviour
{
    private PlayerMovement movement;
    private AttackController attackCtrl;
    private PlayerHealth healthCtrl;
    private InputHandler input;
    private SimpleAI ai;

    public Transform target;
    public bool isAI = false;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        attackCtrl = GetComponent<AttackController>();
        healthCtrl = GetComponent<PlayerHealth>();
        input = GetComponent<InputHandler>();
        ai = GetComponent<SimpleAI>();
    }

    private void Update()
    {
        if (healthCtrl.IsDead) return;

        if (isAI && ai != null)
        {
            ai.HandleAI(target, movement, attackCtrl);
        }
        else if (input != null)
        {
            HandlePlayerInput();
        }
    }

    private void HandlePlayerInput()
    {
        movement.Move(input.Horizontal);
        if (input.JumpPressed) movement.Jump();
        if (input.AttackPressed) attackCtrl.TryAttack(target);
        if (input.DashPressed) movement.Dash();

        input.ResetInput();
    }
}
