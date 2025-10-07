using System.Collections;
using UnityEngine;

public class EnemyJESTER : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float pushDuration = 0.6f;
    public float pushSpeed = 5f;
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

    public static EnemyJESTER currentAttacker = null;

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

        // Abaikan tabrakan antar musuh dan dengan player
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int playerLayer = LayerMask.NameToLayer("Player");
        if (enemyLayer >= 0 && playerLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
            Physics2D.IgnoreLayerCollision(enemyLayer, playerLayer, true);
        }
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
        else stayTimer = 0f;

        if (!isAttacking)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            Vector2 separation = GetSeparationForce();
            Vector2 finalDir = (direction + separation).normalized;

            rb.MovePosition(rb.position + finalDir * moveSpeed * Time.deltaTime);

            // 🚫 Cegah tumpukan berat antar Jester
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, separationDistance * 0.8f);
            foreach (var col in overlaps)
            {
                if (col != null && col.CompareTag("Enemy") && col.gameObject != gameObject)
                {
                    Vector2 pushDir = (transform.position - col.transform.position).normalized;
                    rb.MovePosition(rb.position + pushDir * 0.02f); // dorong dikit biar misah
                }
            }

            if (spriteRenderer != null)
                spriteRenderer.flipX = direction.x < 0;

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

        if (currentAttacker == null && player != null)
        {
            currentAttacker = this;
            yield return StartCoroutine(PushPlayer());

            var playerScript = player.GetComponent<PlayerControlerxcf>();
            if (playerScript != null)
                playerScript.TakeDamage(damageAmount);

            currentAttacker = null;
        }

        isAttacking = false;
    }

    IEnumerator PushPlayer()
    {
        float elapsed = 0f;
        Vector2 direction = (player.position - transform.position).normalized;

        while (elapsed < pushDuration && player != null)
        {
            Vector2 targetPos = (Vector2)player.position + direction * 0.8f;
            player.position = Vector2.MoveTowards(player.position, targetPos, pushSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // 🔧 versi baru: separation pakai physics overlap
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
        if (currentAttacker == this)
            currentAttacker = null;
        Destroy(gameObject);

        EnemySpawnerJESTER spawner = FindObjectOfType<EnemySpawnerJESTER>();
        if (spawner != null)
            spawner.EnemyDied();
    }
}
