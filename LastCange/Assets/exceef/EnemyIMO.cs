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
    public float attackStartDelay = 1f; // delay sebelum monster pertama kali menyerang
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
    //     STATIC SHARED ATTACK LOCK
    // ================================
    public static EnemyIMO currentAttacker; // shared antar semua monster

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        monster = GetComponent<Monster>();

        if (tutorialManager == null)
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
        //   NOTIFY TUTORIAL (1x)
        // ================================
        if (!hasNotifiedTutorial && dist < 1.2f)
        {
            hasNotifiedTutorial = true;
            if (tutorialManager != null)
                tutorialManager.NotifyMonsterTouchedPlayer(this);

            Debug.Log($"📩 {name} lapor ke TutorialManager!");
        }

        if (!canMove)
            return; // freeze movement if tutorial paused

        // ================================
        //           MOVEMENT
        // ================================
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

    void TryAttack()
    {
        if (!canAttack)
            return;

        // delay sebelum monster pertama kali menyerang
        if (!attackStarted)
        {
            if (Time.time < lastAttackTime + attackStartDelay)
                return;
            attackStarted = true;
            lastAttackTime = Time.time;
        }

        // cek cooldown
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        // cek apakah monster lain sedang menyerang
        if (currentAttacker != null && currentAttacker != this)
            return;

        // mulai serang
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

        // release slot attacker setelah cooldown
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

    // ================================
    //   TUTORIAL CONTROL METHODS
    // ================================
    public void SetTutorialManager(TutorialManager tm)
    {
        tutorialManager = tm;
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
}
