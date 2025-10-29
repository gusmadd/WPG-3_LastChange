using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Monster : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 3;
    private int currentHP;

    [Header("Death")]
    public GameObject deathEffect; // opsional: drag particle efek mati
    private Animator anim;

    void Start()
    {
        currentHP = maxHP;
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"{gameObject.name} kena {damage} damage! HP tersisa: {currentHP}");

        if (anim != null)
            anim.SetTrigger("Hit");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} mati!");
        if (anim != null)
            anim.SetTrigger("Die");

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        // beri delay dikit kalau mau animasi dulu
        Destroy(gameObject, 0.3f);
    }
}
