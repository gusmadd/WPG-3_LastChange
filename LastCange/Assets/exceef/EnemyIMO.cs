using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyIMO : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float pullDuration = 0.6f;
    public float pullSpeed = 5f;
    public float separationDistance = 1.2f;
    public float separationForce = 3f;

    [Header("Attack Settings")]
    public int damageAmount = 1;
    public float attackCooldown = 1.5f;
    public float damageDelay = 0.5f;
    public float stayTimeToTrigger = 1.2f;

    private float lastAttackTime;
    private bool isAttacking = false;
    private bool playerInside = false;
    private float stayTimer = 0f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Collider2D coll;
    private bool facingRight = false;

    public static EnemyIMO currentPuller = null;
    public bool canAttack = false;
    private bool hasNotifiedTutorial = false;
    private TutorialManager tutorialManager;
    private bool canMove = true;

    [Header("Tutorial Mode")]
    public bool noDamageInTutorial = false;

    private int debugID;

    void Start()
    {
        // ✅ Setup layer collision
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int playerLayer = LayerMask.NameToLayer("Player");
        int mapLayer = LayerMask.NameToLayer("Map");

        if (enemyLayer >= 0)
        {
            // ❌ Abaikan antar musuh & player
            Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
            Physics2D.IgnoreLayerCollision(enemyLayer, playerLayer, true);

            // ✅ Tapi JANGAN abaikan map (biar nabrak)
            Physics2D.IgnoreLayerCollision(enemyLayer, mapLayer, false);
        }

        // ✅ Komponen dasar
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();

        debugID = Random.Range(1000, 9999);
        Debug.Log($"🧠 EnemyIMO #{debugID} Start() → noDamageInTutorial = {noDamageInTutorial}, canAttack = {canAttack}");

        if (coll != null)
            coll.isTrigger = true;

        // ✅ Cari player otomatis
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;
        if (!canMove)
        {
            if (anim != null) anim.SetBool("isMoving", false);
            return;
        }

        if (playerInside && !isAttacking)
        {
            stayTimer += Time.deltaTime;
            if (stayTimer >= stayTimeToTrigger && Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(Attack());
                stayTimer = 0f;
            }
        }
        else
        {
            stayTimer = 0f;
        }

        if (!isAttacking)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
        }
    }

    IEnumerator Attack()
    {
        if (SceneManager.GetActiveScene().name == "Level 0")
        {
            Debug.Log($"[#{debugID}] ❌ Attack dibatalkan karena sedang di scene tutorial (Level 0).");
            yield break;
        }

        if (!canAttack)
            yield break;

        isAttacking = true;
        lastAttackTime = Time.time;

        if (anim != null)
            anim.SetTrigger("Attack");

        yield return new WaitForSeconds(damageDelay);

        if (currentPuller == null && player != null)
        {
            currentPuller = this;
            var playerScript = player.GetComponent<PlayerControler>();

            Debug.Log($"💥 [#{debugID}] Menyerang player di scene {SceneManager.GetActiveScene().name}");

            yield return StartCoroutine(PullPlayer());

            if (playerScript != null)
            {
                playerScript.TakeDamage(damageAmount);
            }

            currentPuller = null;
        }

        isAttacking = false;
    }

    IEnumerator PullPlayer()
    {
        if (noDamageInTutorial)
        {
            Debug.Log($"🌀 [#{debugID}] PullPlayer() dibatalkan (tutorial mode).");
            yield break;
        }

        float elapsed = 0f;
        Vector2 startPos = player.position;
        Vector2 targetPos = transform.position + (Vector3)(transform.right * -0.5f);

        while (elapsed < pullDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pullDuration;
            player.position = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        if (!hasNotifiedTutorial)
        {
            hasNotifiedTutorial = true;
            var tut = FindObjectOfType<TutorialManager>();
            if (tut != null)
            {
                tut.NotifyMonsterTouchedPlayer(this);
                Debug.Log($"📩 [#{debugID}] Lapor ke TutorialManager (first contact).");
            }
        }

        if (canAttack && !isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            if (noDamageInTutorial)
            {
                Debug.Log($"[#{debugID}] ❌ Serangan diblokir karena masih tutorial mode.");
                return;
            }

            StartCoroutine(Attack());
        }
    }

    public void SetTutorialManager(TutorialManager t)
    {
        tutorialManager = t;
    }

    public void StopMovement()
    {
        canMove = false;
        rb.velocity = Vector2.zero;
    }

    public void ResumeMovement()
    {
        canMove = true;
    }
}
