using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControlerxcf : MonoBehaviour
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

    [Header("Burning")]
    public bool isBurning = false;      // apakah player terbakar
    public bool hasFlame = false;       // apakah player bawa api untuk lilin
    public float burnDamagePerSecond = 10f;

    [Header("UI Pain Bar")]
    public Image painFill; 
    public GameObject painBarUI; 

    [Header("Respawn")]
    public Transform spawnPoint; // drag empty object ke sini di Inspector

    private Animator anim;
    private bool nearFire = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        currentHP = maxHP;
        currentPain = maxPain;

        if (painBarUI != null)
            painBarUI.SetActive(false);
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

        // Update animator parameter
        anim.SetBool("isBurning", isBurning);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    void Burn()
    {
        currentPain -= burnDamagePerSecond * Time.deltaTime;
        if (currentPain <= 0)
        {
            DieGosong();
        }
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        Debug.Log("Player kena damage! Sisa HP: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void StartBurning()
    {
        if (!isBurning)
        {
            isBurning = true;
            hasFlame = true;
            Debug.Log("Player mulai terbakar (pakai E)!");
        }
        
        if (painBarUI != null)
        {
            painBarUI.SetActive(true);
            painFill.fillAmount = 1f;
        }
    }

    public void StopBurning()
    {
        isBurning = false;
        hasFlame = false;
        Debug.Log("Api padam.");
        if (painBarUI != null)
            painBarUI.SetActive(false);
    }

    void DieGosong()
    {
        Debug.Log("Player gosong terbakar!");
        Die();
    }

    void Die()
    {
        Debug.Log("U Dead"); // console log
        Respawn();
    }

    void Respawn()
    {
        // Balikin ke spawnPoint
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
        }
        else
        {
            transform.position = Vector3.zero; // fallback kalau spawnPoint gak di-set
        }

        // Reset HP & pain
        currentHP = maxHP;
        currentPain = maxPain;

        // Matikan status terbakar kalau masih nyala
        StopBurning();

        Debug.Log("Respawn di titik awal.");
    }

    private void OnTriggerEnter2D(Collider2D other)
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
}
