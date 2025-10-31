using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(FighterController))]
public class InputHandler : MonoBehaviour
{
    private InputSystem_Actions controls;

    public float Horizontal { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool DashPressed { get; private set; }  // ✅ Added Dash

    private void Awake()
    {
        controls = new InputSystem_Actions();

        controls.Player.Move.performed += ctx => Horizontal = ctx.ReadValue<Vector2>().x;
        controls.Player.Move.canceled += ctx => Horizontal = 0;

        controls.Player.Jump.performed += ctx => JumpPressed = true;
        controls.Player.Jump.canceled += ctx => JumpPressed = false;

        controls.Player.Attack.performed += ctx => AttackPressed = true;
        controls.Player.Attack.canceled += ctx => AttackPressed = false;

        controls.Player.Dash.performed += ctx => DashPressed = true;
        controls.Player.Dash.canceled += ctx => DashPressed = false;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    public void ResetInput()
    {
        JumpPressed = false;
        AttackPressed = false;
        DashPressed = false;  // ✅ Reset Dash too
    }
}
