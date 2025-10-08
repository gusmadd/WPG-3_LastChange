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

    // shared (global antar semua lilin)
    private static bool portalActivated = false;
    private static GameObject staticPortal;
    private static Image staticProgressBar;
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
        GameManager.Instance.PlaySFX(GameManager.Instance.lilinNyalaSFX);
        Debug.Log("🔥 Lilin menyala!");
        CheckAllCandles();
    }

    void CheckAllCandles()
    {
        foreach (Candle c in allCandles)
        {
            if (!c.isLit) return;
        }

        // Semua lilin nyala → aktifkan portal (sekali aja)
        if (!portalActivated && victoryPortal != null)
        {
            victoryPortal.SetActive(true);
            portalActivated = true;
            staticPortal = victoryPortal;
            staticProgressBar = portalProgressBar;
            Debug.Log("✨ Semua lilin menyala! Portal aktif!");
        }
    }

    void Update()
    {
        if (!portalActivated || staticPortal == null) return;

        // deteksi player di portal
        Collider2D[] hits = Physics2D.OverlapCircleAll(staticPortal.transform.position, 1.5f, LayerMask.GetMask("Player"));
        playerInsidePortal = false;

        foreach (Collider2D col in hits)
        {
            // hanya tanggapi objek bertag "Player"
            if (col.CompareTag("Player"))
            {
                playerInsidePortal = true;
                break;
            }
        }


        // update timer dan bar
        if (playerInsidePortal)
        {
            playerStayTimer += Time.deltaTime;

            if (staticProgressBar != null)
            {
                staticProgressBar.gameObject.SetActive(true);
                staticProgressBar.fillAmount = playerStayTimer / portalStayTime;

                staticProgressBar.transform.position = Camera.main.WorldToScreenPoint(
                    staticPortal.transform.position + progressBarOffset
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

            if (staticProgressBar != null)
            {
                staticProgressBar.fillAmount = 0;
                staticProgressBar.gameObject.SetActive(false);
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
