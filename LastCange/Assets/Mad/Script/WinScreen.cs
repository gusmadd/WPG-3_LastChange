using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public Image circleTransition;    // lingkaran pembuka
    public Image toBeContinuedImage;  // gambar "To Be Continued"
    public Image pressSpaceImage;     // gambar "Press Space"
    public Image fadeOutImage;        // layer hitam transparan untuk fade out

    [Header("Settings")]
    public float circleExpandDuration = 1.5f;
    public float toBeSlideDuration = 1.0f;
    public float pressScaleDuration = 0.6f;
    public float fadeOutDuration = 1.2f;
    public string nextSceneName = "Main Menu";

    private bool canPressSpace = false;
    private bool isFadingOut = false;

    void Start()
    {
        // --- Setup posisi awal ---
        if (circleTransition != null)
            circleTransition.rectTransform.localScale = Vector3.zero;

        if (toBeContinuedImage != null)
        {
            Vector3 pos = toBeContinuedImage.rectTransform.anchoredPosition;
            pos.x = -800f; // mulai dari kiri
            toBeContinuedImage.rectTransform.anchoredPosition = pos;

            var cg = toBeContinuedImage.GetComponent<CanvasGroup>();
            if (cg == null) cg = toBeContinuedImage.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0;
        }

        if (pressSpaceImage != null)
        {
            pressSpaceImage.rectTransform.localScale = Vector3.zero;
            var cg = pressSpaceImage.GetComponent<CanvasGroup>();
            if (cg == null) cg = pressSpaceImage.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0;
        }

        if (fadeOutImage != null)
        {
            Color c = fadeOutImage.color;
            c.a = 0f;
            fadeOutImage.color = c;
        }

        StartCoroutine(PlayWinSequence());
    }

    IEnumerator PlayWinSequence()
    {
        // 1️⃣ Lingkaran membesar dari kecil ke besar
        if (circleTransition != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / circleExpandDuration;
                float scale = Mathf.SmoothStep(0f, 15f, t);
                circleTransition.rectTransform.localScale = Vector3.one * scale;
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.3f);

        // 2️⃣ “To Be Continued” meluncur dari kiri
        if (toBeContinuedImage != null)
        {
            float t = 0f;
            Vector3 startPos = new Vector3(-800f, 0f, 0f);
            Vector3 endPos = new Vector3(0f, 0f, 0f);
            CanvasGroup cg = toBeContinuedImage.GetComponent<CanvasGroup>();

            while (t < 1f)
            {
                t += Time.deltaTime / toBeSlideDuration;
                toBeContinuedImage.rectTransform.anchoredPosition =
                    Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, t));
                if (cg != null) cg.alpha = Mathf.Clamp01(t);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.8f);

        // 3️⃣ “Press Space” muncul dari kecil ke besar
        if (pressSpaceImage != null)
        {
            float t = 0f;
            CanvasGroup cg = pressSpaceImage.GetComponent<CanvasGroup>();
            while (t < 1f)
            {
                t += Time.deltaTime / pressScaleDuration;
                float scale = Mathf.SmoothStep(0f, 1f, t);
                pressSpaceImage.rectTransform.localScale = Vector3.one * scale;
                if (cg != null) cg.alpha = Mathf.Clamp01(t);
                yield return null;
            }

            // mulai blinking
            StartCoroutine(BlinkPressSpace());
        }

        yield return new WaitForSeconds(0.5f);
        canPressSpace = true;
    }

    IEnumerator BlinkPressSpace()
    {
        CanvasGroup cg = pressSpaceImage.GetComponent<CanvasGroup>();
        while (true)
        {
            if (cg != null)
                cg.alpha = Mathf.PingPong(Time.time * 1.5f, 1f);
            yield return null;
        }
    }

    void Update()
    {
        if (canPressSpace && !isFadingOut && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(FadeOutAndLoad());
        }
    }

    IEnumerator FadeOutAndLoad()
    {
        isFadingOut = true;
        if (fadeOutImage != null)
        {
            float t = 0f;
            Color c = fadeOutImage.color;
            while (t < 1f)
            {
                t += Time.deltaTime / fadeOutDuration;
                c.a = Mathf.SmoothStep(0f, 1f, t);
                fadeOutImage.color = c;
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(nextSceneName);
    }
}