using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyIMO : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;

    [Header("Attack")]
    public int damage = 1;
    public float attackCooldown = 1f;
    public float attackStartDelay = 1f;
    private float lastAttackTime;
    private bool attackStarted = false;

    [Header("Animation")]
    public Animator anim;

    private Monster monster;
    private Rigidbody2D rb;
    private Vector2 moveDir;

    // ================================  
    //     TUTORIAL VARIABLES  
    // ================================
    public bool noDamageInTutorial = false;
    private bool hasNotifiedTutorial = false;
    private TutorialManager tutorialManager;

    private bool canMove = true;
    public bool canAttack = true;

    // ================================
    //     GLOBAL ATTACK LOCK
    // ================================
    public static EnemyIMO currentAttacker;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        monster = GetComponent<Monster>();

        tutorialManager = FindObjectOfType<TutorialManager>();

        if (player == null)
            player = FindObjectOfType<PlayerControler>().transform;
    }

    void Update()
    {
        if (monster != null && monster.IsDead())
            return;

        if (player == null)
            return;

        float dist = Vector2.Distance(transform.position, player.position);

        // ================================
        //   REMOVE OLD DISTANCE DETECT!
        // ================================
        // (Sudah dihapus agar tidak double detect)

        if (!canMove)
            return;

        // MOVEMENT
        if (dist > attackRange)
        {
            moveDir = (player.position - transform.position).normalized;
            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.deltaTime);

            if (anim != null)
                anim.SetBool("isWalking", true);

            Flip(moveDir.x);
        }
        else
        {
            if (anim != null)
                anim.SetBool("isWalking", false);

            TryAttack();
        }
    }

    // ====================================
    //   FIX: DETEKSI TUTORIAL PAKAI COLLISION
    // ====================================
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasNotifiedTutorial) return;
        if (!collision.collider.CompareTag("Player")) return;
        if (tutorialManager == null) return;

        hasNotifiedTutorial = true;
        tutorialManager.NotifyMonsterTouchedPlayer(this);

        Debug.Log($"📩 {name} memberi sinyal TUTORIAL: kontak fisik dengan Player!");
    }

    // ====================================
    //             ATTACK
    // ====================================
    void TryAttack()
    {
        if (!canAttack)
            return;

        if (!attackStarted)
        {
            if (Time.time < lastAttackTime + attackStartDelay)
                return;

            attackStarted = true;
            lastAttackTime = Time.time;
        }

        if (Time.time < lastAttackTime + attackCooldown)
            return;

        if (currentAttacker != null && currentAttacker != this)
            return;

        currentAttacker = this;
        lastAttackTime = Time.time;

        if (anim != null)
            anim.SetTrigger("Attack");

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange)
        {
            if (!noDamageInTutorial)
            {
                PlayerControler pc = player.GetComponent<PlayerControler>();
                if (pc != null)
                {
                    pc.TakeDamage(damage);
                    Debug.Log($"{name} menyerang player!");
                }
            }
        }

        StartCoroutine(ReleaseAttackerAfterCooldown());
    }

    IEnumerator ReleaseAttackerAfterCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        if (currentAttacker == this)
            currentAttacker = null;
    }

    void Flip(float xDir)
    {
        if (xDir < 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (xDir > 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    public void StopMovement()
    {
        canMove = false;
        rb.velocity = Vector2.zero;
        if (anim != null)
            anim.SetBool("isWalking", false);
    }

    public void ResumeMovement()
    {
        canMove = true;
    }
    public void SetTutorialManager(TutorialManager tm)
    {
        tutorialManager = tm;
    }
}