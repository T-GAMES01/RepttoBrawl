using UnityEngine;

public enum ControlType { Local, AI, Online }

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerInputHandler : MonoBehaviour
{
    public ControlType controlType = ControlType.Local;
    private InputHandler input;
    private PlayerMovement movement;
    private PlayerHealth health;
    private SimpleAI ai;

    void Awake()
    {
        input = GetComponent<InputHandler>();
        movement = GetComponent<PlayerMovement>();
        health = GetComponent<PlayerHealth>();
        ai = GetComponent<SimpleAI>();
    }

    void Update()
    {
        switch (controlType)
        {
            case ControlType.Local:
                HandleLocalInput();
                break;
            case ControlType.AI:
                HandleAIInput();
                break;
            case ControlType.Online:
                HandleNetworkInput();
                break;
        }
    }

    void HandleLocalInput()
    {
        if (input == null) return;

        // Movement handled by PlayerMovement FixedUpdate
        if (input.AttackPressed)
        {
            GetComponent<AttackController>()?.DoAttack();
            input.ResetAttack();
        }
    }

    void HandleAIInput()
    {
        if (ai == null) return;

        ai.UpdateAI();
    }

    void HandleNetworkInput()
    {
        // TODO: Photon or Mirror implementation
    }
}
