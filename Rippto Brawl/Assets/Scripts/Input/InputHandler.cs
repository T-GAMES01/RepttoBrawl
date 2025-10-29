using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private InputSystem_Actions controls;
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool AttackPressed { get; private set; }

    private void Awake()
    {
        controls = new InputSystem_Actions();

        // Movement
        controls.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        // Jump
        controls.Player.Jump.performed += ctx => JumpPressed = true;
        controls.Player.Jump.canceled += ctx => JumpPressed = false;

        // Attack
        controls.Player.Attack.performed += ctx => AttackPressed = true;
        controls.Player.Attack.canceled += ctx => AttackPressed = false;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    // ✅ Add this helper
    public void ResetJump()
    {
        JumpPressed = false;
    }
}
