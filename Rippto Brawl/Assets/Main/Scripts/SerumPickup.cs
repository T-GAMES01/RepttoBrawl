using UnityEngine;

public class SerumPickup : MonoBehaviour
{
    private SerumSpawn spawner;

    private void Start()
    {
        spawner = FindAnyObjectByType<SerumSpawn>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // your player tag
        {
            spawner.OnSerumPicked();
            Destroy(gameObject);
        }
    }
}
