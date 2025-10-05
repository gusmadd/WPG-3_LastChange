using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candle : MonoBehaviour
{
    public GameObject flame; // sprite api di atas lilin
    private bool isLit = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isLit) return; // kalau sudah nyala, skip

        PlayerControler player = collision.GetComponent<PlayerControler>();
        if (player != null && player.isBurning) // pastikan player terbakar
        {
            LightCandle(player);
        }
    }

    void LightCandle(PlayerControler player)
    {
        isLit = true;
        flame.SetActive(true);   // hidupkan sprite api lilin
        player.StopBurning();    // player otomatis padam setelah nyalain lilin
        Debug.Log("Candle lit!");
    }
}
