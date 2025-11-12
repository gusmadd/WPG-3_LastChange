using System.Collections;
using UnityEngine;

public class EnemyIMO : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;

    [Header("Attack Settings")]
    public int damageAmount = 1;
    public float attackCooldown = 1.5f;
    public float damageDelay = 0.5f;

    [Header("VFX")]
    public float flashDuration = 0.15f;
    private Color originalColor;

    private float lastAttackTime;
    private bool isAttacking = false;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    [Header("Tutorial Mode")]
    public bool noDamageInTutorial = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
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

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(Attack());
        }
        else
        {
            // Movement
            Vector2 direction = (player.position - transform.position).normalized;
            rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);

            if (anim != null)
                anim.SetBool("isMoving", true);
        }
    }

    // Hanya ada 1 Attack coroutine, gak ada duplikat
    IEnumerator Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        if (anim != null) anim.SetTrigger("Attack");

        yield return new WaitForSeconds(damageDelay);

        var playerScript = player.GetComponent<PlayerControler>();
        if (playerScript != null && !noDamageInTutorial)
        {
            playerScript.TakeDamage(damageAmount);
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    public void TakeDamage(int damage)
    {
        StartCoroutine(FlashRed());
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
}
