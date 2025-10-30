using UnityEngine;

[CreateAssetMenu(menuName = "Game/Global Constants", fileName = "GlobalConstants")]
public class GlobalConstants : ScriptableObject
{
    private static GlobalConstants _instance;

    public static GlobalConstants Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GlobalConstants>("GlobalConstants");

#if UNITY_EDITOR
                if (_instance == null)
                {
                    Debug.LogWarning("GlobalConstants.asset not found in Resources folder! Creating temporary instance.");
                    _instance = CreateInstance<GlobalConstants>();
                }
#endif
            }
            return _instance;
        }
    }

    // 🔹 LAYERS
    [HideInInspector] public int LAYER_GROUND;

    // 🔹 DEFAULT PLAYER SETTINGS
    [Header("Player Settings")]
    public float MOVE_SPEED = 5f;
    public float JUMP_FORCE = 7f;
    public int MAX_JUMPS = 2;

    // 🔹 DASH SETTINGS
    [Header("Dash Settings")]
    public float DASH_SPEED = 15f;
    public float DASH_DURATION = 0.2f;
    public float DASH_COOLDOWN = 0.5f;

    // 🔹 DEBUG OPTION
    [Header("Debug Options")]
    public bool ENABLE_DEBUG = true;

    private void OnEnable()
    {
        // ✅ Update runtime layer value safely
        LAYER_GROUND = LayerMask.NameToLayer("Ground");
    }

    // ✅ Helper function for safe logging
    public void Log(string message)
    {
        if (ENABLE_DEBUG)
            Debug.Log($"[GlobalConstants] {message}");
    }
}
