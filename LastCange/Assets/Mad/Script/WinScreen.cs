using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviour
{
    void Update()
    {
        // Kalau pemain menekan spasi
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Ganti "Menu" dengan nama scene menu kamu di Project
            SceneManager.LoadScene("Main Menu");
        }
    }
}
