using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject creditsPanel;
    public GameObject optionsPanel;

    [Header("Audio")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    private AudioSource loopSFXSource;
    public AudioClip mainMenuBGM;
    public AudioClip level1BGM;
    public AudioClip level0BGM;
    public AudioClip buttonClickSFX;
    public AudioClip lilinNyalaSFX;

    [Header("SFX - Player")]
    public AudioClip burnStartSFX;
    public AudioClip burnLoopSFX;
    public AudioClip burnEndSFX;
    public AudioClip attackSFX;

    [Header("Option Buttons")]
    public Button musicButton;
    public Button sfxButton;

    [Header("Music Sprites")]
    public Sprite musicIdle;
    public Sprite musicHover;
    public Sprite musicPressed;
    public Sprite musicToggled;
    [Header("SFX Sprites")]
    public Sprite sfxIdle;
    public Sprite sfxHover;
    public Sprite sfxPressed;
    public Sprite sfxToggled;

    private bool musicMuted = false;
    private bool sfxMuted = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Buat source loop SFX
            loopSFXSource = gameObject.AddComponent<AudioSource>();
            loopSFXSource.loop = true;
            loopSFXSource.volume = 0.5f;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Fungsi tambahan:
    public void PlayLoopSFX(AudioClip clip)
    {
        if (clip == null) return;
        loopSFXSource.clip = clip;
        loopSFXSource.Play();
    }
    public void StopLoopSFX()
    {
        loopSFXSource.Stop();
    }

    private void Start()
    {
        PlayBGM(mainMenuBGM);

        // Cegah error jika tombol belum diset
        if (musicButton != null) musicButton.onClick.AddListener(ToggleMusic);
        if (sfxButton != null) sfxButton.onClick.AddListener(ToggleSFX);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ganti BGM berdasarkan scene
        switch (scene.name)
        {
            case "MainMenu":
                PlayBGM(mainMenuBGM);
                break;
            case "Level 0":
                PlayBGM(level0BGM);
                break;
            case "Level 1":
                PlayBGM(level1BGM);
                break;
        }
    }

    // ---------------- AUDIO CONTROL ----------------

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip == clip) return; // biar gak restart lagu sama
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.volume = 0.4f;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (!sfxMuted && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void ToggleMusic()
    {
        PlaySFX(buttonClickSFX);
        musicMuted = !musicMuted;
        bgmSource.mute = musicMuted;
        UpdateMusicButton();
    }

    public void ToggleSFX()
    {
        PlaySFX(buttonClickSFX);
        sfxMuted = !sfxMuted;
        sfxSource.mute = sfxMuted;
        loopSFXSource.mute = sfxMuted; 
        UpdateSFXButton();
    }

    void UpdateMusicButton()
    {
        if (musicButton == null) return;
        var img = musicButton.GetComponent<Image>();
        img.sprite = musicMuted ? musicToggled : musicIdle;
    }

    void UpdateSFXButton()
    {
        if (sfxButton == null) return;
        var img = sfxButton.GetComponent<Image>();
        img.sprite = sfxMuted ? sfxToggled : sfxIdle;
    }

    // ---------------- SCENE MANAGEMENT ----------------

    public void PlayGame()
    {
        PlaySFX(buttonClickSFX);
        SceneManager.LoadScene("Level 0");
    }

    public void LoadNextLevel()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene + 1);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        PlaySFX(buttonClickSFX);
        Debug.Log("Quit Game");
        Application.Quit();
    }

    // ---------------- MENU PANELS ----------------

    public void ShowCredits()
    {
        PlaySFX(buttonClickSFX);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void BackToOptions()
    {
        PlaySFX(buttonClickSFX);
        creditsPanel.SetActive(false);
        optionsPanel.SetActive(true);
        mainMenuPanel.SetActive(true);
    }
    public void BackToMenu()
    {
        PlaySFX(buttonClickSFX);
        creditsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ShowOptions()
    {
        PlaySFX(buttonClickSFX);
        optionsPanel.SetActive(true);
        UpdateMusicButton();
        UpdateSFXButton();
    }
}