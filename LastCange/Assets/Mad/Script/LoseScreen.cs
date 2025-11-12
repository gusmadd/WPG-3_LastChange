using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public Image background;       // hitam full screen
    public Image toBeImage;        // "To Be Continued"
    public Image pressImage;       // "Press Space"

    [Header("Transition Settings")]
    public float bgExpandDuration = 1.2f;
    public float toBeSlideDuration = 1f;
    public float pressPopDuration = 0.6f;
    public float blinkInterval = 0.5f;

    [Header("Scene Settings")]
    public string menuSceneName = "Main Menu";

    private bool canPress = false;

    void OnEnable()
    {
        // mulai animasi saat panel aktif
        StartCoroutine(PlayLoseSequence());
    }

    void Update()
    {
        // tekan spasi hanya jika animasi sudah selesai
        if (canPress && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(FadeOutAndLoadMenu());
        }
    }

    IEnumerator PlayLoseSequence()
    {
        // 1️⃣ Awal semua diset transparan/kecil
        background.fillAmount = 0f;
        toBeImage.color = new Color(1, 1, 1, 0);
        pressImage.color = new Color(1, 1, 1, 0);
        pressImage.transform.localScale = Vector3.zero;

        // 2️⃣ Animasi background dari lingkaran kecil ke full
        float t = 0f;
        while (t < bgExpandDuration)
        {
            t += Time.deltaTime;
            background.fillAmount = Mathf.Lerp(0f, 1f, t / bgExpandDuration);
            yield return null;
        }

        // 3️⃣ Munculkan To Be Continued dari kiri
        t = 0f;
        Vector3 startPos = toBeImage.rectTransform.localPosition + new Vector3(-800, 0, 0);
        Vector3 endPos = toBeImage.rectTransform.localPosition;
        toBeImage.rectTransform.localPosition = startPos;

        while (t < toBeSlideDuration)
        {
            t += Time.deltaTime;
            float progress = t / toBeSlideDuration;
            toBeImage.rectTransform.localPosition = Vector3.Lerp(startPos, endPos, progress);
            toBeImage.color = new Color(1, 1, 1, progress);
            yield return null;
        }

        // 4️⃣ "Press Space" muncul dari kecil ke besar
        t = 0f;
        while (t < pressPopDuration)
        {
            t += Time.deltaTime;
            float progress = t / pressPopDuration;
            pressImage.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, progress);
            pressImage.color = new Color(1, 1, 1, progress);
            yield return null;
        }

        // 5️⃣ Mulai berkedip
        StartCoroutine(BlinkPress());

        // 6️⃣ Baru boleh tekan spasi
        yield return new WaitForSeconds(1f);
        canPress = true;
    }

    IEnumerator BlinkPress()
    {
        while (true)
        {
            yield return new WaitForSeconds(blinkInterval);
            pressImage.enabled = !pressImage.enabled;
        }
    }

    IEnumerator FadeOutAndLoadMenu()
    {
        canPress = false;
        float fadeDuration = 1f;
        float t = 0f;

        Color bgColor = background.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            bgColor.a = alpha;
            background.color = bgColor;
            toBeImage.color = new Color(1, 1, 1, alpha);
            pressImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        SceneManager.LoadScene(menuSceneName);
    }
}