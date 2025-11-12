using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class Monster : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public float moveSpeed = 2f;
    public float attackRange = 2.5f;
    public int damageAmount = 1;
    public float attackCooldown = 1.5f;
    public float damageDelay = 0.5f;

    [Header("Stats")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("VFX")]
    public float flashDuration = 0.15f;
    private Color originalColor;

    [Header("Separation (Optional)")]
    public float separationRadius = 1.2f;
    public float separationStrength = 2f;
    public string targetTag = "Enemy";
    public LayerMask obstacleMask;

    [Header("Tutorial Mode")]
    public bool noDamageInTutorial = false;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private float lastAttackTime;
    private bool isAttacking = false;
    private bool canAttack = true;
    private int debugID;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        int enemyLayer = LayerMask.NameToLayer("Monster");
        int playerLayer = LayerMask.NameToLayer("Player");
        int mapLayer = LayerMask.NameToLayer("Map");

        Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
        Physics2D.IgnoreLayerCollision(enemyLayer, playerLayer, true);
        Physics2D.IgnoreLayerCollision(enemyLayer, mapLayer, false);

        currentHealth = maxHealth; // 👈 inisialisasi HP
        debugID = Random.Range(1000, 9999);
        Debug.Log($"🧠 Monster #{debugID} Awake() done. HP={currentHealth}");
    }

    void Update()
    {
        if (player == null || isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Serang kalau dekat
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(Attack());
        }
        else
        {
            // Gerak ke player
            Vector2 direction = (player.position - transform.position).normalized;
            Vector2 separation = ComputeSeparation();
            Vector2 moveDir = (direction + separation).normalized;

            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.deltaTime);

            if (anim != null)
                anim.SetBool("isMoving", moveDir.magnitude > 0.1f);
        }
    }

    IEnumerator Attack()
    {
        if (isAttacking) yield break;

        isAttacking = true;
        lastAttackTime = Time.time;

        if (anim != null)
            anim.SetTrigger("Attack");

        yield return new WaitForSeconds(damageDelay);

        if (player != null)
        {
            var playerScript = player.GetComponent<PlayerControler>();
            if (playerScript != null)
            {
                Debug.Log($"💥 Monster #{debugID} menyerang player langsung!");
                playerScript.TakeDamage(damageAmount);
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    // 🩸 Monster menerima damage
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"💢 Monster #{debugID} kena damage {damage}, sisa HP = {currentHealth}");

        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    void Die()
    {
        Debug.Log($"☠️ Monster #{debugID} mati.");
        canAttack = false;
        isAttacking = false;

        if (anim != null)
            anim.SetTrigger("Die");

        rb.velocity = Vector2.zero;
        rb.simulated = false; // nonaktifkan physics biar gak tergeser
        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, 0.6f); // delay sedikit untuk animasi mati
    }

    private Vector2 ComputeSeparation()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, separationRadius);
        Vector2 totalForce = Vector2.zero;
        int count = 0;

        foreach (Collider2D col in nearby)
        {
            if (col == null || col.gameObject == gameObject) continue;
            if (!col.CompareTag(targetTag)) continue;

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
            RaycastHit2D hit = Physics2D.Raycast(transform.position, totalForce.normalized, totalForce.magnitude + 0.1f, obstacleMask);
            if (hit.collider == null)
                return totalForce.normalized;
        }

        return Vector2.zero;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
