using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;

    private bool _isGamePaused = false;

    [SerializeField] private Material[] skybox;
    private int skyboxIndex = 0;

    [SerializeField] private AudioSource playerSounds;

    [SerializeField] private AudioClip winSound;

    [SerializeField] private AudioSource themeMusic;

    public event EventHandler OnWinning;
    private bool wasMenuPressed;
    private bool isMenuPressedNow;

    private void Awake()
    {
        Instance = this;

    }
    private void Start()
    {
        _isGamePaused = true;
        TogglePauseGame();
        themeMusic = GameObject.Find("Music").GetComponent<AudioSource>();
        themeMusic.Play();
    }

    private void Update()
    {
        tryGetMenuButton();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseGame();
        }
    }

    private void tryGetMenuButton()
    {
        isMenuPressedNow = ExcavatorCustomInputController.Instance.leftController.TryGetFeatureValue(CommonUsages.menuButton, out bool menuBool);
        if (isMenuPressedNow && !wasMenuPressed)
        {
            TogglePauseGame();
        }
        wasMenuPressed = isMenuPressedNow;
    }

    public void TogglePauseGame()
    {
        _isGamePaused = !_isGamePaused;
        if (_isGamePaused)
        {
            Time.timeScale = 0f;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
            //MouseLooking.instance.enabled = false;
        } else
        {
            Time.timeScale = 1f;
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
            //MouseLooking.instance.enabled = true;
        }
    }

    public void skyboxChange()
    {
        skyboxIndex++;
        
        
        if (skyboxIndex >= skybox.Length)
        {
            skyboxIndex = 0;
        }
        RenderSettings.skybox = skybox[skyboxIndex];
    }

    public string getMaterialName()
    {
        return "THEME: " + skybox[skyboxIndex].name.ToUpper();
    }

    public void playSound(AudioClip soundToPlay)
    {
        playerSounds.PlayOneShot(soundToPlay);
    }

    public void LevelComplete()
    {
        themeMusic.Stop();
        playSound(winSound);
        OnWinning?.Invoke(this, EventArgs.Empty);

        float newScore = ExcavatorController.Instance.getScore();
        if (!PlayerPrefs.HasKey("hiScore"))
        {
            PlayerPrefs.SetFloat("hiScore", 0);
        }
        float highScore = PlayerPrefs.GetFloat("hiScore");

        if (PlayerPrefs.HasKey("hiScore"))
        {
            if (newScore > PlayerPrefs.GetFloat("hiScore"))
            {
                highScore = newScore;
                PlayerPrefs.SetFloat("hiScore", highScore);
                PlayerPrefs.Save();
            }
        }
        else
        {
            if (newScore > highScore)
            {
                highScore = newScore;
                PlayerPrefs.SetFloat("hiScore", highScore);
                PlayerPrefs.Save();
            }
        }
    }
}
