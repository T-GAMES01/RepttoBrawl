using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab; // 🔹 local player
    public GameObject aiPrefab;     // 🔹 AI enemy

    [Header("Spawn Points")]
    public Transform spawnPointLocal;
    public Transform spawnPointAI;

    private GameObject p1, p2;

    private void Start()
    {
        SpawnPlayers();
    }

    void SpawnPlayers()
    {
        // ✅ safety check
        if (playerPrefab == null || aiPrefab == null)
        {
            Debug.LogError("❌ Prefab missing in PlayerSpawner!");
            return;
        }

        // ✅ Spawn local player
        p1 = Instantiate(playerPrefab, spawnPointLocal.position, Quaternion.identity);
        p1.name = "Player_Local";

        // ✅ Spawn AI player
        p2 = Instantiate(aiPrefab, spawnPointAI.position, Quaternion.identity);
        p2.name = "Player_AI";

        // ✅ connect AI target
        var aiScript = p2.GetComponent<SimpleAI>();
        if (aiScript != null)
            aiScript.target = p1.transform;
        else
            Debug.LogWarning("⚠️ SimpleAI script missing on AI prefab!");

        Debug.Log("✅ Players Spawned Successfully!");
    }
}
