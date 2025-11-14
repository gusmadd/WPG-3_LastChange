using UnityEngine;

public class Monster : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 3;
    private int currentHP;

    [Header("Animation")]
    public Animator anim;

    private bool isDead = false;

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHP -= amount;
        Debug.Log($"{name} kena hit! Sisa HP: {currentHP}");

        if (anim != null)
            anim.SetTrigger("Hit");

        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;

        Debug.Log($"{name} mati!");

        if (anim != null)
            anim.SetTrigger("Die");

        Destroy(gameObject, 0.8f);
    }

    public bool IsDead()
    {
        return isDead;
    }
}
