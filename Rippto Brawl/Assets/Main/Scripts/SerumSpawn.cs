using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SerumSpawn : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject serumPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Timing")]
    [SerializeField] private float respawnDelay = 5f;   // time after pickup
    [SerializeField] private float maxLifetime = 20f;   // auto despawn if ignored

    private int activeSerums = 0;
    private int initialSpawnCount = 4;
    private int normalSpawnCount = 2;

    private void Start()
    {
        // Spawn 4 at the beginning
        SpawnSerums(initialSpawnCount);
    }

    // --------------------------------------------------------
    // SPAWN N SERUMS
    // --------------------------------------------------------
    private void SpawnSerums(int amount)
    {
        if (spawnPoints.Length == 0) return;

        for (int i = 0; i < amount; i++)
        {
            Transform p = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject serum = Instantiate(serumPrefab, p.position, Quaternion.identity);

            activeSerums++;

            // Auto despawn if ignored
            StartCoroutine(AutoDespawn(serum));
        }
    }

    // --------------------------------------------------------
    // WHEN PLAYER PICKS THE SERUM, CALL THIS FROM THE SERUM SCRIPT
    // --------------------------------------------------------
    public void OnSerumPicked()
    {
        activeSerums--;

        // If it was from the first 4 batch → only spawn new when all 4 are gone
        if (activeSerums == 0)
            StartCoroutine(SpawnAfterDelay(normalSpawnCount));
    }

    private IEnumerator SpawnAfterDelay(int count)
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnSerums(count);
    }

    // --------------------------------------------------------
    // AUTO DESPAWN IF NOT PICKED
    // --------------------------------------------------------
    private IEnumerator AutoDespawn(GameObject serum)
    {
        yield return new WaitForSeconds(maxLifetime);

        if (serum != null)
        {
            Destroy(serum);
            OnSerumPicked();
        }
    }
}
