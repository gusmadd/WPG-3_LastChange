using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    public string nextSceneName = "NextScene";
    public float stayTime = 5f;
    public Vector3 uiOffset = new Vector3(0, 2f, 0);
    public float scaleInDuration = 0.5f;

    [Header("Portal Detection")]
    // 👇 Ganti 3.0f yang tadinya hardcoded dengan variabel ini
    public float maxDetectionRadius = 3.0f;

    [Header("Linked Candles")]
    public Candle[] candles;

    [Header("UI Elements")]
    public GameObject portalUI;
    public Image fillBar;
    public Image emptyBar;
    public Image border;

    private bool allCandlesLit = false;
    private float timer = 0f;
    private bool playerInside = false;
    private List<SpriteRenderer> portalRenderers;
    private Vector3 originalScale; // Variabel ini sudah ada dan akan digunakan

    void Start()
    {
        portalRenderers = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
        if (portalRenderers.Count > 0)
        {
            originalScale = portalRenderers[0].transform.localScale;
        }
        else
        {
            originalScale = transform.localScale;
        }

        SetPortalVisuals(false);
        transform.localScale = Vector3.zero;

        if (portalUI != null)
            portalUI.SetActive(false);
    }

    void Update()
    {
        // cek lilin
        if (!allCandlesLit && AllCandlesLit())
        {
            allCandlesLit = true;
            Debug.Log("✨ Semua lilin menyala, portal aktif! Mulai animasi scale-up.");

            // 🔥 Matikan semua lilin
            foreach (var c in candles)
            {
                if (c != null)
                    c.gameObject.SetActive(false);
            }

            StartCoroutine(AnimatePortalScaleIn());
        }

        // kalau belum aktif, keluar
        if (!allCandlesLit)
            return;

        // --- LOGIKA DETEKSI YANG DIMODIFIKASI ---

        // Hitung radius deteksi saat ini
        float scaleRatio = (originalScale.x != 0) ? transform.localScale.x / originalScale.x : 0f;
        float currentRadius = maxDetectionRadius * scaleRatio;

        if (currentRadius < 0.01f)
        {
            currentRadius = 0.01f;
        }

        // Gunakan LayerMask untuk HANYA mendeteksi objek yang berada di Layer "Player"
        int playerLayerMask = LayerMask.GetMask("Player");

        // OverlapCircleAll HANYA akan mengembalikan hits dari Layer "Player"
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentRadius, playerLayerMask);

        playerInside = false;

        foreach (var hit in hits)
        {
            // Meskipun sudah difilter Layer, menambahkan tag check adalah praktik bagus
            // untuk memastikan hanya objek Player yang benar-benar memicu.
            if (hit.CompareTag("Player"))
            {
                playerInside = true;
                break; // Pemain terdeteksi
            }
        }
        //END LOGIKA YANG DIMODIFIKASI
        // update UI progress bar
        UpdatePortalUI();

        // logic timer
        if (playerInside)
        {
            timer += Time.deltaTime;
            if (timer >= stayTime)
            {
                Debug.Log("🎯 Player bertahan penuh di portal! Lanjut ke scene: " + nextSceneName);
                SceneManager.LoadScene(nextSceneName);
            }
        }
        else
        {
            if (timer > 0)
            {
                timer = 0;
                Debug.Log("❌ Player keluar dari portal, timer reset.");
            }
        }
    }

    IEnumerator AnimatePortalScaleIn()
    {
        SetPortalVisuals(true);
        if (portalUI != null)
            portalUI.SetActive(true);

        float elapsedTime = 0f;

        while (elapsedTime < scaleInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / scaleInDuration;
            t = t * t * (3f - 2f * t); // SmoothStep 

            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);

            yield return null;
        }

        transform.localScale = originalScale;
    }

    void SetPortalVisuals(bool active)
    {
        foreach (var r in portalRenderers)
        {
            if (r != null)
                r.enabled = active;
        }
    }

    void UpdatePortalUI()
    {
        if (portalUI == null) return;

        portalUI.SetActive(allCandlesLit && playerInside);

        if (!allCandlesLit || !playerInside)
            return;

        portalUI.transform.position = Camera.main.WorldToScreenPoint(transform.position + uiOffset);

        if (fillBar != null)
            fillBar.fillAmount = timer / stayTime;

        if (emptyBar != null)
            emptyBar.fillAmount = 1f;

        if (border != null)
            border.enabled = true;
    }

    bool AllCandlesLit()
    {
        foreach (var c in candles)
        {
            if (c == null || !c.isLit)
                return false;
        }
        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // 👇 Gambar Gizmo menggunakan radius maksimum (untuk visualisasi editor)
        Gizmos.DrawWireSphere(transform.position, maxDetectionRadius);
    }
}