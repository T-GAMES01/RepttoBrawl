using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Stats")]
    public float DamagePercent = 0f;

    [Header("KO Settings")]
    public float maxFallY = -10f; // fall limit
    public Transform[] respawnPoints; // platform spawn points
    public DroneRespawn dronePrefab; // reference to drone prefab

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CheckOutOfBounds();
    }

    public void TakeDamage(float amount)
    {
        DamagePercent += amount;
        Debug.Log(gameObject.name + " %Damage: " + DamagePercent);
    }

    private void CheckOutOfBounds()
    {
        if (transform.position.y < maxFallY)
        {
            KO();
        }
    }

    private void KO()
    {
        Debug.Log(gameObject.name + " KO'd!");

        // Stop player movement
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true; // disable physics while drone picks up

        // Spawn Drone to pick up player
        if (dronePrefab != null && respawnPoints.Length > 0)
        {
            DroneRespawn drone = Instantiate(dronePrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            drone.StartPickup(this, respawnPoints[Random.Range(0, respawnPoints.Length)]);
        }
        else
        {
            // Fallback respawn
            RespawnAtRandomPoint();
        }
    }

    public void RespawnAtRandomPoint()
    {
        // Reset damage percent
        DamagePercent = 0;

        // Pick random platform
        if (respawnPoints.Length > 0)
        {
            int rand = Random.Range(0, respawnPoints.Length);
            transform.position = respawnPoints[rand].position;
        }
        else
        {
            transform.position = Vector3.zero; // fallback
        }

        // Enable physics
        rb.isKinematic = false;
        rb.linearVelocity = Vector2.zero;
    }
}
