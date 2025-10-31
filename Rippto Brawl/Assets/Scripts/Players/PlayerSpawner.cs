using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject aiPrefab;
    public Transform playerSpawnPoint;
    public Transform aiSpawnPoint;

    void Start()
    {
        GameObject player = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
        GameObject ai = Instantiate(aiPrefab, aiSpawnPoint.position, Quaternion.identity);

        FighterController playerCtrl = player.GetComponent<FighterController>();
        FighterController aiCtrl = ai.GetComponent<FighterController>();

        playerCtrl.target = ai.transform;
        aiCtrl.target = player.transform;
        aiCtrl.isAI = true;
    }
}
