using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool GameOver { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void EndGame(string winner)
    {
        GameOver = true;
        Debug.Log("Game Over! Winner: " + winner);
    }
}
