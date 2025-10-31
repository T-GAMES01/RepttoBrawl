using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void PlayerDied(PlayerHealth player)
    {
        GlobalConstants.Instance.Log($"☠️ {player.name} Died!");
        // TODO: win condition check
    }
}
