using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public int maxHP = 3;
    private int currentHP;

    public float moveSpeed = 2f;
    public float attackCooldown = 2f;   // cooldown antar serangan
    public float damageDelay = 1.5f;    // delay sebelum damage kena

    private float lastAttackTime;
    private bool playerInRange = false; // flag apakah player masih kontak

    private Transform player;
    private Animator anim;
    void Start()
    {
        currentHP = maxHP;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;

            anim.SetBool("isWalking", direction.magnitude > 0.01f);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerInRange = true;
            if (Time.time > lastAttackTime + attackCooldown)
            {
                anim.SetTrigger("Attack"); // anim langsung jalan
                StartCoroutine(DealDamageAfterDelay(collision.gameObject));
                lastAttackTime = Time.time;
            }
        }
    }

    IEnumerator DealDamageAfterDelay(GameObject target)
    {
        yield return new WaitForSeconds(damageDelay);

        if (target != null && playerInRange) // cek masih kontak atau tidak
        {
            PlayerControler pc = target.GetComponent<PlayerControler>();
            if (pc != null)
            {
                pc.TakeDamage(1);
                Debug.Log("Monster kasih damage setelah delay (masih kontak)!");
            }
        }
        else
        {
            Debug.Log("Damage dibatalkan, player sudah kabur!");
        }
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        Debug.Log("Monster kena damage, sisa HP: " + currentHP);

        if (currentHP <= 0) Die();
    }

    void Die()
    {
        Debug.Log("Monster mati!");
        Destroy(gameObject);
    }
}

