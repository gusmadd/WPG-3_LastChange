using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal; // <— penting!

public class Candle : MonoBehaviour
{
    [Header("Candle Settings")]
    public GameObject flame;
    public bool isLit = false;

    [Header("Light Effect")]
    public Light2D candleLight;   // <— drag Light2D ke sini di Inspector

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerControler player = collision.GetComponent<PlayerControler>();
        if (player != null && player.isBurning && !isLit)
        {
            LightCandle(player);
        }
    }

    void LightCandle(PlayerControler player)
    {
        isLit = true;
        flame.SetActive(true);

        // 🔥 Aktifkan cahaya
        if (candleLight != null)
            candleLight.enabled = true;

        player.StopBurning();

        GameManager.Instance.PlaySFX(GameManager.Instance.lilinNyalaSFX);
        Debug.Log($"🔥 {name} menyala!");
    }
}