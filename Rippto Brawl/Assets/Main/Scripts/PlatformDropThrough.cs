using UnityEngine;
using System.Collections;

public class PlatformDropThrough : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField] private float dropDuration = 0.4f; // how long collisions are ignored
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float maxDropDetectDistance = 2f; // max distance to detect platform below
    [SerializeField] private Vector2 boxSize = new Vector2(0.64f, 300f); // fixed box size

    private Collider2D playerCollider;
    private Rigidbody2D rb;

    private void Awake()
    {
        playerCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Must be in-air
        bool isInAir = rb.linearVelocity.y != 0f;

        if (isInAir && (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)))
        {
            DropThroughNextPlatform();
        }
    }

    private void DropThroughNextPlatform()
    {
        // Constantly check for the closest platform below
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, boxSize, 0f, Vector2.down, maxDropDetectDistance, groundLayer);

        if (hit.collider != null && hit.collider.TryGetComponent(out PlatformEffector2D effector))
        {
            StartCoroutine(DisableCollisionTemporarily(hit.collider));
        }
    }

    private IEnumerator DisableCollisionTemporarily(Collider2D ground)
    {
        Physics2D.IgnoreCollision(playerCollider, ground, true);
        yield return new WaitForSeconds(dropDuration);
        Physics2D.IgnoreCollision(playerCollider, ground, false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Vector3 start = transform.position;
        Vector3 end = transform.position + Vector3.down * maxDropDetectDistance;
        Gizmos.DrawWireCube(start, boxSize);
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireCube(end, boxSize);
    }
}
