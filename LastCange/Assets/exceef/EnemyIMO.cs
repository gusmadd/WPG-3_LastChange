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
    private float lastAttackTime;

    [Header("Animation")]
    public Animator anim;

    private Monster monster; // HP script
    private Rigidbody2D rb;
    private Vector2 moveDir;

    // ================================  
    //     TUTORIAL VARIABLES  
    // ================================
    public bool noDamageInTutorial = false;   // toggle dari luar
    private bool hasNotifiedTutorial = false; // laporan 1x ke TutorialManager
    private TutorialManager tutorialManager;

    private bool canMove = true; // kontrol movement
    public bool canAttack = true; // kontrol attack dari TutorialManager

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

        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;

        if (anim != null)
            anim.SetTrigger("Attack");

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange)
        {
            if (noDamageInTutorial)
            {
                Debug.Log($"❌ {name} tidak memberi damage (tutorial mode).");
                return;
            }

            PlayerControler pc = player.GetComponent<PlayerControler>();
            if (pc != null)
            {
                pc.TakeDamage(damage);
                Debug.Log($"{name} menyerang player!");
            }
        }
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
