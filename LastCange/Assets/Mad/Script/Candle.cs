using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Candle : MonoBehaviour
{
    [Header("Candle Settings")]
    public GameObject flame; // sprite api
    private bool isLit = false;
    public static List<Candle> allCandles = new List<Candle>();

    [Header("Portal Settings")]
    public GameObject victoryPortal; // portal diset inactive di scene
    public string nextSceneName = "NextScene";
    public float portalStayTime = 5f;
    public Image portalProgressBar; // drag UI Image (fill type radial/horizontal)
    public Vector3 progressBarOffset = new Vector3(0, 2f, 0);

    private static bool portalActivated = false;
    private static float playerStayTimer = 0f;
    private static bool playerInsidePortal = false;

    void Awake()
    {
        allCandles.Add(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isLit) return;

        PlayerControler player = collision.GetComponent<PlayerControler>();
        if (player != null && player.isBurning)
        {
            LightCandle(player);
        }
    }

    void LightCandle(PlayerControler player)
    {
        isLit = true;
        flame.SetActive(true);
        player.StopBurning();
        Debug.Log("🔥 Lilin menyala!");
        CheckAllCandles();
    }

    void CheckAllCandles()
    {
        foreach (Candle c in allCandles)
        {
            if (!c.isLit) return;
        }

        // Semua lilin nyala → aktifkan portal
        if (!portalActivated && victoryPortal != null)
        {
            victoryPortal.SetActive(true);
            portalActivated = true;
            Debug.Log("✨ Semua lilin menyala! Portal aktif!");
        }
    }

    void Update()
    {
        if (!portalActivated || victoryPortal == null) return;

        // deteksi player di portal
        Collider2D[] hits = Physics2D.OverlapCircleAll(victoryPortal.transform.position, 1.5f);
        playerInsidePortal = false;

        foreach (Collider2D col in hits)
        {
            if (col.CompareTag("Player"))
            {
                playerInsidePortal = true;
                break;
            }
        }

        if (playerInsidePortal)
        {
            playerStayTimer += Time.deltaTime;

            // update progress bar UI
            if (portalProgressBar != null)
            {
                portalProgressBar.gameObject.SetActive(true);
                portalProgressBar.fillAmount = playerStayTimer / portalStayTime;

                // posisi bar mengikuti portal
                portalProgressBar.transform.position = Camera.main.WorldToScreenPoint(
                    victoryPortal.transform.position + progressBarOffset
                );
            }

            if (playerStayTimer >= portalStayTime)
            {
                Debug.Log("🎯 Player bertahan penuh di portal! Lanjut scene...");
                SceneManager.LoadScene(nextSceneName);
            }
        }
        else
        {
            if (playerStayTimer > 0)
            {
                Debug.Log("❌ Player keluar dari portal, timer reset.");
                playerStayTimer = 0;
            }

            if (portalProgressBar != null)
            {
                portalProgressBar.fillAmount = 0;
                portalProgressBar.gameObject.SetActive(false);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (victoryPortal != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(victoryPortal.transform.position, 1.5f);
        }
    }
}
