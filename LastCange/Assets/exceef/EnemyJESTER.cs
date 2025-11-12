using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
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
    public float attackCooldown = 1.5f;
    public float damageDelay = 0.5f;
    public float stayTimeToTrigger = 1.2f;

    [Header("VFX")]
    public float flashDuration = 0.15f;

    [Header("Collision")]
    public LayerMask obstacleMask;

    private float lastAttackTime;
    private bool isAttacking = false;
    private bool playerInside = false;
    private float stayTimer = 0f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Color originalColor;
    private bool facingRight = true;
    public static EnemyJESTER currentAttacker = null;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        rb.freezeRotation = true;

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
    }

    void Update()
    {
        if (player == null || isAttacking) return;

        // Hitung jarak ke player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Kalau player dekat, langsung serang
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(Attack());
        }
        else
        {
            // Gerak ke player
            Vector2 direction = (player.position - transform.position).normalized;
            Vector2 separation = GetSeparationForce();
            Vector2 finalDir = (direction + separation).normalized;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, finalDir, 0.6f, obstacleMask);
            if (hit.collider == null)
            {
                rb.MovePosition(rb.position + finalDir * moveSpeed * Time.deltaTime);
                if (anim != null) anim.SetBool("isMoving", true);
            }
            else if (anim != null) anim.SetBool("isMoving", false);

            AutoFlip();
        }
    }

    void AutoFlip()
    {
        if (player.position.x > transform.position.x && !facingRight) Flip();
        else if (player.position.x < transform.position.x && facingRight) Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        if (anim != null) anim.SetTrigger("Attack");
        yield return new WaitForSeconds(damageDelay);

        if (currentAttacker == null && player != null)
        {
            currentAttacker = this;

            // 🔥 Panggil Monster script untuk deal damage
            Monster monsterScript = GetComponent<Monster>();
            if (monsterScript != null)
            {
                monsterScript.SendMessage("DealDamageToPlayer", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                // fallback kalau Monster ga ada
                PlayerControler playerScript = player.GetComponent<PlayerControler>();
                if (playerScript != null)
                    playerScript.TakeDamage(1); // default damage
            }

            yield return StartCoroutine(PushPlayer());
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
        if (collision.CompareTag("Player")) playerInside = true;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = false;
            stayTimer = 0f;
        }
    }

    public void TakeDamage(int damage)
    {
        StartCoroutine(FlashRed());
    }

    IEnumerator FlashRed()
    {
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            sr.color = originalColor;
        }
    }

    public void Die()
    {
        if (currentAttacker == this) currentAttacker = null;
        Destroy(gameObject);

        EnemySpawnerJESTER spawner = FindObjectOfType<EnemySpawnerJESTER>();
        if (spawner != null) spawner.EnemyDied();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationDistance);
    }
}
