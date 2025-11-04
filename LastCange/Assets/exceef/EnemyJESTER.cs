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
    public static EnemyJESTER currentAttacker = null;
    public LayerMask obstacleMask;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

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

        Vector2 direction = (player.position - transform.position).normalized;
        Vector2 separation = GetSeparationForce();
        Vector2 finalDir = (direction + separation).normalized;

        float checkDist = 0.6f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, finalDir, checkDist, obstacleMask);

        if (hit.collider == null)
        {
            rb.MovePosition(rb.position + finalDir * moveSpeed * Time.deltaTime);
            if (anim != null) anim.SetBool("isMoving", true);
        }
        else
        {
            if (anim != null) anim.SetBool("isMoving", false);
        }

        if (playerInside && Time.time >= lastAttackTime + attackCooldown)
        {
            stayTimer += Time.deltaTime;
            if (stayTimer >= stayTimeToTrigger)
            {
                StartCoroutine(Attack());
                stayTimer = 0f;
            }
        }
        else stayTimer = 0f;
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

            var playerScript = player.GetComponent<PlayerControler>();
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationDistance);
    }
}
