using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Playercontrollerxcf : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    [Header("Animation (Optional)")]
    public Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // biar ga jatuh kalo 2D top-down
        rb.gravityScale = 0;

        if (anim == null)
            anim = GetComponent<Animator>();
    }

    void Update()
    {
        // ambil input arah
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize(); // biar diagonal ga lebih cepet

        // update animasi (kalau ada)
        if (anim != null)
        {
            bool isMoving = moveInput.magnitude > 0.1f;
            anim.SetBool("isWalking", isMoving);
        }
    }

    void FixedUpdate()
    {
        // gerakin player
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}
