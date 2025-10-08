using System.Collections;
using UnityEngine;

public class EnemyIMO : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float pullDuration = 0.6f;
    public float pullSpeed = 5f;
    public float separationDistance = 1.2f;
    public float separationForce = 3f;

    [Header("Attack Settings")]
    public int damageAmount = 1;
    public float attackCooldown = 1.5f;
    public float damageDelay = 0.5f;
    public float stayTimeToTrigger = 1.2f;

    private float lastAttackTime;
    private bool isAttacking = false;
    private bool playerInside = false;
    private float stayTimer = 0f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool isAttacking = false;
=======
=======
    private Collider2D coll;


    public static EnemyIMO currentPuller = null;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();

        // Collider dijadikan trigger agar tidak nabrak fisik
        if (coll != null)
            coll.isTrigger = true;

        // Abaikan collision antar Enemy & Player
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int playerLayer = LayerMask.NameToLayer("Player");

        if (enemyLayer >= 0 && playerLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
            Physics2D.IgnoreLayerCollision(enemyLayer, playerLayer, true);
        }

        // Cari player otomatis jika belum diset
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }
    }
    void Flip()
    {
        facingRight = !facingRight;

        // ambil skala sekarang
        Vector3 scale = transform.localScale;
        scale.x *= -1; // balik sumbu X
        transform.localScale = scale;
    }

    void Update()
    {
        if (player == null) return;

        if (playerInside && !isAttacking)
        {
            stayTimer += Time.deltaTime;
            if (stayTimer >= stayTimeToTrigger && Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(Attack());
                stayTimer = 0f;
            }
        }
        else
        {
            stayTimer = 0f;
        }

        if (!isAttacking)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);

            if (spriteRenderer != null)
                spriteRenderer.flipX = direction.x < 0;

=======
            Vector2 separation = GetSeparationForce();
            Vector2 finalDir = (direction + separation).normalized;

            rb.MovePosition(rb.position + finalDir * moveSpeed * Time.deltaTime);
            // --- Tambahkan ini di bawah gerakan ---
            if (finalDir.x > 0.1f && !facingRight)
            {
                Flip();
            }
            else if (finalDir.x < -0.1f && facingRight)
            {
                Flip();
            }


            // 🚫 Cegah tumpukan berat (push out sedikit)
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, separationDistance * 0.8f);
            foreach (var col in overlaps)
            {
                if (col != null && col.CompareTag("Enemy") && col.gameObject != gameObject)
                {
                    Vector2 pushDir = (transform.position - col.transform.position).normalized;
                    rb.MovePosition(rb.position + pushDir * 0.02f); // dorong dikit biar misah
                }
            }

            if (anim != null)
                anim.SetBool("isMoving", true);
        }
        else
        {
            if (anim != null)
                anim.SetBool("isMoving", false);
        }
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        if (anim != null)
            anim.SetTrigger("Attack");

        yield return new WaitForSeconds(damageDelay);

        if (currentPuller == null && player != null)
        {
            currentPuller = this;

            // damage player
            var playerScript = player.GetComponent<PlayerControlerxcf>();
=======
            yield return StartCoroutine(PullPlayer());

            var playerScript = player.GetComponent<PlayerControler>();

            if (playerScript != null)
                playerScript.TakeDamage(damageAmount);

            currentPuller = null;
        }

        isAttacking = false;
    }

    IEnumerator PullPlayer()
    {
        float elapsed = 0f;
        Vector2 startPos = player.position;

        while (elapsed < pullDuration && player != null)
        {
            Vector2 targetPos = Vector2.Lerp(startPos, transform.position, elapsed / pullDuration);
            player.position = Vector2.MoveTowards(player.position, targetPos, pullSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // 🌀 Perhitungan separation pakai overlap (lebih halus)
    Vector2 GetSeparationForce()
    {
        Vector2 force = Vector2.zero;
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, separationDistance);

        foreach (Collider2D col in nearby)
        {
            if (col == null || col.gameObject == gameObject) continue;
            if (!col.CompareTag("Enemy")) continue;

            Vector2 away = (Vector2)(transform.position - col.transform.position);
            float dist = away.magnitude;
            if (dist < 0.01f) continue;

            away.Normalize();
            force += away * (separationForce / dist);
        }

        return force;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInside = true;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = false;
            stayTimer = 0f;
        }
    }

    public void Die()
    {
        if (currentPuller == this)
            currentPuller = null;

        Destroy(gameObject);

        // Panggil spawner baru
        EnemySpawnerIMO spawner = FindObjectOfType<EnemySpawnerIMO>();
        if (spawner != null)
            spawner.EnemyDied();
    }

    // 🧭 Debug radius separation
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationDistance);
    }
}
