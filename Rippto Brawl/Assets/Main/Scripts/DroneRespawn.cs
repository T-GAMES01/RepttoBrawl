using System.Collections;
using UnityEngine;

public class DroneRespawn : MonoBehaviour
{
    public float speed = 8f; 
    public float smoothTime = 0.2f;
    public float pickupHeight = 2f;
    public float dropDelay = 0.5f;

    private Transform player;
    private Transform targetPlatform;

    private Vector3 velocity = Vector3.zero;

    public void StartPickup(PlayerStats playerStats, Transform platform)
    {
        player = playerStats.transform;
        targetPlatform = platform;
        StartCoroutine(PickupCoroutine(playerStats));
    }

    private IEnumerator PickupCoroutine(PlayerStats playerStats)
    {
        // === Move drone above player smoothly ===
        Vector3 targetPos = player.position + Vector3.up * pickupHeight;

        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPos,
                ref velocity,
                smoothTime,
                speed
            );

            yield return null;
        }

        // === Attach player smoothly ===
        player.SetParent(transform);

        // === Now fly to platform ===
        Vector3 platformPos = targetPlatform.position + Vector3.up * pickupHeight;

        while (Vector3.Distance(transform.position, platformPos) > 0.05f)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                platformPos,
                ref velocity,
                smoothTime,
                speed
            );

            yield return null;
        }

        // Pause before drop
        yield return new WaitForSeconds(dropDelay);

        // === Drop player ===
        player.SetParent(null);
        playerStats.RespawnAtRandomPoint();

        Destroy(gameObject);
    }
}