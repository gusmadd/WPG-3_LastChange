using UnityEngine;

public class EnemySeparation : MonoBehaviour
{
    public float separationRadius = 1.2f;
    public float separationStrength = 2f;
    public string enemyTag = "Enemy";
    public LayerMask obstacleMask; // 🎯 Tambahin ini di Inspector, centang layer "Map"

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
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
        {
            totalForce /= count;
            Vector2 moveDir = totalForce.normalized;
            float moveDist = totalForce.magnitude * separationStrength * Time.fixedDeltaTime;

            // 🚧 Cek dulu apakah ada obstacle di depan
            RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, moveDist + 0.1f, obstacleMask);
            if (hit.collider == null)
            {
                // ✅ Aman, baru gerak
                rb.MovePosition(rb.position + moveDir * moveDist);
            }
            else
            {
                // 🚫 Ada obstacle, stop biar gak nembus
                // (optional) bisa tambahin dorongan kecil ke arah samping
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
