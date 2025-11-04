using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemySeparation : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float separationRadius = 1.2f;
    public float separationStrength = 2f;
    public string enemyTag = "Enemy";
    public LayerMask obstacleMask; // 🎯 Centang "Map" dan "Obstacle" di inspector
    public Transform player;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // 🔹 Gerak ke arah player
        Vector2 toPlayer = (player.position - transform.position).normalized;
        Vector2 desiredVelocity = toPlayer * moveSpeed;

        // 🔹 Tambah efek separation
        Vector2 separationForce = CalculateSeparation();
        desiredVelocity += separationForce * separationStrength;

        // 🔹 Cek obstacle depan
        RaycastHit2D hit = Physics2D.Raycast(transform.position, desiredVelocity.normalized, 0.3f, obstacleMask);
        if (hit.collider != null)
        {
            // 🚫 Kena obstacle, stop
            rb.velocity = Vector2.zero;
        }
        else
        {
            rb.velocity = desiredVelocity;
        }
    }

    Vector2 CalculateSeparation()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, separationRadius);
        Vector2 totalForce = Vector2.zero;
        int count = 0;

        foreach (Collider2D col in nearby)
        {
            if (col == null || col.gameObject == gameObject) continue;
            if (!col.CompareTag(enemyTag)) continue;

            Vector2 diff = (Vector2)(transform.position - col.transform.position);
            float dist = diff.magnitude;
            if (dist < 0.001f) continue;

            diff.Normalize();
            totalForce += diff / dist;
            count++;
        }

        if (count > 0)
            totalForce /= count;

        return totalForce;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log($"{name} nabrak player!");
            // contoh damage:
            var playerCtrl = collision.collider.GetComponent<PlayerControler>();
            if (playerCtrl != null)
                playerCtrl.TakeDamage(1);
        }

        if (collision.collider.CompareTag("Obstacle") || collision.collider.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            Debug.Log($"{name} nabrak {collision.collider.name}!");
            rb.velocity = Vector2.zero;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
