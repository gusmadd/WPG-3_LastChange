using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Monster : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 3;
    private int currentHP;

    [Header("Movement & Attack")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    public float damageDelay = 0.8f;
    public Transform attackPoint;

    private float lastAttackTime;
    private bool isAttacking = false;

    private Transform player;
    private Animator anim;
    private Rigidbody2D rb;

    void Start()
    {
        currentHP = maxHP;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // pastikan properti fisika benar
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (player == null || isAttacking) return;

        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(attackPoint.position, player.position);

        if (distance > attackRange)
        {
            // gunakan MovePosition agar bisa tabrakan dengan collider lain
            Vector2 newPos = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
            anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isWalking", false);

            if (Time.time > lastAttackTime + attackCooldown)
            {
                StartCoroutine(AttackWithDelay());
                lastAttackTime = Time.time;
            }
        }

        // arahkan hadap
        if (direction.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction.x > 0 ? -1 : 1);
            transform.localScale = scale;
        }
    }

    IEnumerator AttackWithDelay()
    {
        isAttacking = true;
        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(damageDelay);

        float currentDistance = Vector2.Distance(attackPoint.position, player.position);
        if (currentDistance <= attackRange)
        {
            PlayerControler pc = player.GetComponent<PlayerControler>();
            if (pc != null)
            {
                pc.TakeDamage(1);
                Debug.Log("⚔ Monster berhasil menyerang!");
            }
        }
        else
        {
            Debug.Log("❌ Player berhasil menghindar sebelum serangan kena!");
        }

        isAttacking = false;
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }

    // 🔍 Visualisasi jarak serang
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) attackPoint = transform;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
