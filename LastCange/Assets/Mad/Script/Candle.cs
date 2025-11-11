using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Candle : MonoBehaviour
{
    [Header("Candle Settings")]
    public GameObject flame;
    public bool isLit = false;

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
        player.StopBurning();

        GameManager.Instance.PlaySFX(GameManager.Instance.lilinNyalaSFX);
        Debug.Log($"🔥 {name} menyala!");
    }
}
