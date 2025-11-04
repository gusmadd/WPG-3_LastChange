using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Monster : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 3;
    private int currentHP;

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
}
