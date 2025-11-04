using System.Collections;
using UnityEngine;

public class SpawnEffect : MonoBehaviour
{
    [Header("Animation Frames")]
    public Sprite[] frames; // 🧩 Masukkan 19 sprite di sini
    public float frameRate = 0.05f; // ⏱️ Durasi tiap frame
    public bool destroyOnEnd = true;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        for (int i = 0; i < frames.Length; i++)
        {
            spriteRenderer.sprite = frames[i];
            yield return new WaitForSeconds(frameRate);
        }

        if (destroyOnEnd)
            Destroy(gameObject);
    }
}
