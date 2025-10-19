using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public Sprite[] frames;          // Semua sprite frame animasi
    public float frameRate = 0.1f;   // Waktu per frame (semakin kecil = makin cepat)
    public bool loop = true;         // Apakah animasi mengulang
    public bool playOnStart = true;  // Main otomatis saat start

    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float timer;
    private bool isPlaying = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (playOnStart)
            Play();
    }

    void Update()
    {
        if (!isPlaying || frames.Length == 0)
            return;

        timer += Time.deltaTime;

        if (timer >= frameRate)
        {
            timer -= frameRate;
            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                if (loop)
                    currentFrame = 0;
                else
                {
                    isPlaying = false;
                    return;
                }
            }

            spriteRenderer.sprite = frames[currentFrame];
        }
    }

    public void Play()
    {
        isPlaying = true;
        currentFrame = 0;
        timer = 0f;
        if (frames.Length > 0)
            spriteRenderer.sprite = frames[currentFrame];
    }

    public void Stop()
    {
        isPlaying = false;
    }

    public void SetFrameRate(float newRate)
    {
        frameRate = newRate;
    }

    public void SetSprites(Sprite[] newFrames)
    {
        frames = newFrames;
    }
}
