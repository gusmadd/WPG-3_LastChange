using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControler : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    [Header("Stats")]
    public int maxHP = 100;
    private int currentHP;
    public float maxPain = 100f;
    private float currentPain;

    [Header("Regen")]
    public float regenSpeed = 10f; // <-- pastikan ini ada (isi per detik)

    [Header("Burning")]
    public bool isBurning = false;      // apakah player terbakar
    public bool hasFlame = false;       // apakah player bawa api untuk lilin
    public float burnDamagePerSecond = 10f;

    [Header("UI Pain Bar")]
    public Image painFill; // drag FillBar (merah)
    public GameObject painBarUI; // drag parent PainBar (yang ada empty + fill)

    [Header("Lives")]
    public int maxLives = 3;
    private int currentLives;
    public Transform spawnPoint; // drag spawn point di inspector

    [Header("UI Heart Sprites")]
    public UnityEngine.UI.Image heartUI; // drag image UI (satu object Image)
    public Sprite heart3; // full (3 nyawa)
    public Sprite heart2;
    public Sprite heart1;
    public Sprite heart0; // kosong

    [Header("Attack")]
    public float attackRange = 1f;      // radius serangan dekat
    public int attackDamage = 1;        // damage ke monster
    public LayerMask monsterLayer;      // layer untuk monster

    [Header("Visual Effect")]
    public SpriteRenderer spriteRenderer;  // drag sprite player di inspector
    public Color attackColor = new Color(1f, 0.3f, 0.3f, 1f); // merah muda
    private Color originalColor;
    public float attackFlashDuration = 0.2f; // durasi efek merah

    private Animator anim;
    private bool nearFire = false; // apakah player dekat api
    void Start()
    {
        currentLives = maxLives;
        UpdateHeartUI();

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        currentHP = maxHP;
        currentPain = maxPain;

        if (painBarUI != null)
            painBarUI.SetActive(false);

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Update()
    {
        // Input
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        // Kalau player terbakar
        if (nearFire && Input.GetKeyDown(KeyCode.E))
        {
            StartBurning();
        }

        // Kalau terbakar → kurangi pain tolerance
        if (isBurning)
        {
            Burn();
        }
        else
        {
            // Kalau tidak terbakar → isi lagi pelan²
            if (currentPain < maxPain)
            {
                currentPain += regenSpeed * Time.deltaTime; // regen pelan²
                if (currentPain > maxPain)
                    currentPain = maxPain;

                // Pastikan bar tetap kelihatan selama belum penuh
                if (painBarUI != null)
                    painBarUI.SetActive(true);
            }
            else
            {
                // Kalau sudah penuh → bar hilang
                if (painBarUI != null)
                    painBarUI.SetActive(false);
            }
        }

        // Update animator parameter
        anim.SetBool("isBurning", isBurning);

        // Update UI selalu ngikut stats
        UpdatePainBar();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    void Burn() //logic kebakar
    {
        currentPain -= burnDamagePerSecond * Time.deltaTime;
        if (currentPain <= 0)
        {
            DieGosong();
        }
    }
    void StartBurning()
    {
        if (!isBurning)
        {
            isBurning = true;
            hasFlame = true;
            Debug.Log("Player mulai terbakar (pakai E)!");
            GameManager.Instance.PlayLoopSFX(GameManager.Instance.burnLoopSFX);
        }

        if (painBarUI != null)
        {
            painBarUI.SetActive(true);
            UpdatePainBar(); // langsung sync ke nilai sekarang
        }
    }

    void UpdatePainBar()
    {
        if (painFill != null)
        {
            float fill = currentPain / maxPain;
            painFill.fillAmount = Mathf.Clamp01(fill); // biar gak minus atau lebih dari 1
        }
    }

    public void StopBurning()
    {
        isBurning = false;
        hasFlame = false;
        Debug.Log("Api padam.");
        GameManager.Instance.StopLoopSFX();
        //GameManager.Instance.PlaySFX(GameManager.Instance.burnEndSFX);
        if (painBarUI != null)
            painBarUI.SetActive(false);
    }

    void DieGosong()
    {
        currentLives--;

        if (currentLives > 0)
        {
            Debug.Log("Player kehilangan 1 nyawa! Sisa: " + currentLives);

            // Reset pain tolerance
            currentPain = maxPain;
            StopBurning();

            // Teleport ke spawn
            if (spawnPoint != null)
                transform.position = spawnPoint.position;
            else
                transform.position = Vector3.zero; // fallback

            UpdateHeartUI();
        }
        else
        {
            Debug.Log("Game Over! Player kehabisan nyawa.");
            UpdateHeartUI();
            // TODO: game over scene / UI
            gameObject.SetActive(false);
        }
    }


    void Die()
    {
        Debug.Log("Player mati (HP habis)");
        // TODO: animasi/game over
    }
    void UpdateHeartUI()
    {
        if (heartUI == null) return;

        switch (currentLives)
        {
            case 3:
                heartUI.sprite = heart3;
                break;
            case 2:
                heartUI.sprite = heart2;
                break;
            case 1:
                heartUI.sprite = heart1;
                break;
            default:
                heartUI.sprite = heart0;
                break;
        }
    }


    private void OnTriggerEnter2D(Collider2D other)  //deteksi sumber api
    {
        if (other.CompareTag("FireSource"))
        {
            nearFire = true;
            Debug.Log("Dekat api. Tekan E untuk terbakar.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("FireSource"))
        {
            nearFire = false;
            Debug.Log("Menjauh dari api.");
        }
    }
    void Attack()
    {
        // mainkan suara serangan
        if (GameManager.Instance != null)
            GameManager.Instance.PlaySFX(GameManager.Instance.attackSFX);
        // cari monster dalam radius
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, monsterLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Monster monster = enemy.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(attackDamage);
            }
        }

        Debug.Log("Player menyerang!");
        StartCoroutine(AttackFlash()); // efek merah
    }
    IEnumerator AttackFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = attackColor;
            yield return new WaitForSeconds(attackFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }
    // buat visual debug di editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    public void TakeDamage(int damage)
    {
        // Kurangi nyawa player
        currentLives -= damage;

        // Update UI heart
        UpdateHeartUI();

        Debug.Log("Player kena damage! Sisa nyawa: " + currentLives);

        if (currentLives <= 0)
        {
            Die();
        }
        else
        {
            // Respawn ke posisi awal (kalau masih ada nyawa)
            if (spawnPoint != null)
                transform.position = spawnPoint.position;
            else
                transform.position = Vector3.zero; // fallback
        }
    }
}
