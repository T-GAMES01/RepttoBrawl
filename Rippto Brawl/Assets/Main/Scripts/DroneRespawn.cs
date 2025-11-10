using System.Collections;
using UnityEngine;

public class DroneRespawn : MonoBehaviour
{
    public float speed = 5f; // movement speed
    public float pickupHeight = 2f; // how high drone lifts player
    public float dropDelay = 0.5f; // wait before releasing player

    private Transform player;
    private Transform targetPlatform;

    public void StartPickup(PlayerStats playerStats, Transform platform)
    {
        player = playerStats.transform;
        targetPlatform = platform;
        StartCoroutine(PickupCoroutine(playerStats));
    }

    private IEnumerator PickupCoroutine(PlayerStats playerStats)
    {
        // Move drone to player
        while (Vector3.Distance(transform.position, player.position + Vector3.up * pickupHeight) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position + Vector3.up * pickupHeight, speed * Time.deltaTime);
            yield return null;
        }

        // Attach player to drone
        player.position = transform.position;

        // Fly to target platform
        while (Vector3.Distance(transform.position, targetPlatform.position + Vector3.up * pickupHeight) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPlatform.position + Vector3.up * pickupHeight, speed * Time.deltaTime);
            player.position = transform.position; // move player with drone
            yield return null;
        }

        // Wait a little before dropping
        yield return new WaitForSeconds(dropDelay);

        // Drop player on platform
        playerStats.RespawnAtRandomPoint();

        // Destroy drone
        Destroy(gameObject);
    }
}
