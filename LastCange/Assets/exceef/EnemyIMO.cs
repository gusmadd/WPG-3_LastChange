using System.Collections;
using UnityEngine;

public class EnemyIMO : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float pullDuration = 0.5f;
    public float pullSpeed = 5f;

    [Header("Attack")]
    public float attackCooldown = 1.5f;
    private float lastAttackTime;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
<<<<<<< Updated upstream
    private bool isAttacking = false;
=======
    private Collider2D coll;
    private bool facingRight = false;
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes

    // 🔑 Biar ga rebutan
    public static EnemyIMO currentPuller = null;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
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

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange && !isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(Attack());
        }
        else if (!isAttacking)
        {
            Vector2 direction = (player.position - transform.position).normalized;
<<<<<<< Updated upstream
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
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
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

        yield return new WaitForSeconds(0.2f); // delay sebelum hit

        // cek: kalau belum ada yg narik, klaim jadi puller
        if (currentPuller == null && player != null)
        {
            currentPuller = this;

<<<<<<< Updated upstream
            // damage player
            var playerScript = player.GetComponent<PlayerControlerxcf>();
=======
            yield return StartCoroutine(PullPlayer());

            var playerScript = player.GetComponent<PlayerControler>();
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
            if (playerScript != null)
                playerScript.TakeDamage(10);

            // tarik player
            yield return StartCoroutine(PullPlayer());

            // selesai narik → bebasin
            currentPuller = null;
        }

        isAttacking = false;
    }

    IEnumerator PullPlayer()
    {
        float elapsed = 0f;
        while (elapsed < pullDuration && player != null)
        {
            player.position = Vector2.MoveTowards(
                player.position,
                transform.position,
                pullSpeed * Time.deltaTime
            );
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public void Die()
    {
        if (currentPuller == this) currentPuller = null; // kalau mati pas narik, lepas giliran
        Destroy(gameObject);

        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
            spawner.EnemyDied();
    }
}
