using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("🎶 BGM Settings")]
    public AudioSource bgmSource;
    public AudioClip defaultBGM;
    [Range(0f, 1f)] public float bgmVolume = 0.6f;

    [Header("🔊 SFX Settings")]
    public AudioSource sfxSource;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    void Awake()
    {
        // Singleton pattern biar nggak dobel
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Auto-setup AudioSources
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.volume = sfxVolume;
        }
    }

    // === BGM Controls ===
    public void PlayBGM(AudioClip clip = null)
    {
        if (clip == null)
            clip = defaultBGM;

        if (clip == null)
        {
            Debug.LogWarning("No BGM clip assigned!");
            return;
        }

        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }

    // === SFX Controls ===
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("Tried to play a null SFX!");
            return;
        }

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
}
