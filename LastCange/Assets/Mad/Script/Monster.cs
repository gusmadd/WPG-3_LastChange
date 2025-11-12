using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Monster : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 3;
    private int currentHP;

    [Header("Attack")]
    public int damage = 1;          // jumlah damage ke player
    public float attackCooldown = 1f; // delay antar serangan
    private bool canAttack = true;

    [Header("Death")]
    public GameObject deathEffect;
    private Animator anim;

    [Header("Facing")]
    public Transform player;
    private bool facingRight = true;

    void Start()
    {
        currentHP = maxHP;
        anim = GetComponent<Animator>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }
    }

    void Update()
    {
        HandleFacing();
    }

    void HandleFacing()
    {
        if (player == null) return;

        float direction = player.position.x - transform.position.x;
        if (direction > 0 && facingRight)
            Flip();
        else if (direction < 0 && !facingRight)
            Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"{gameObject.name} kena {damage} damage! HP tersisa: {currentHP}");

        if (anim != null)
            anim.SetTrigger("Hit");

        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} mati!");
        if (anim != null)
            anim.SetTrigger("Die");

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject, 0.3f);
    }

    // --- serangan ke player ---
    private void OnTriggerEnter2D(Collider2D collision)
{
    Debug.Log($"{gameObject.name} kena trigger sama {collision.name}");

    if (collision.CompareTag("Player") && canAttack)
    {
        var player = collision.GetComponent<PlayerControler>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Debug.Log($"🐊 {gameObject.name} nyerang player dan kasih {damage} damage!");
            StartCoroutine(AttackCooldown());
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerControler gak ketemu di object Player!");
        }
    }
}


    IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}
