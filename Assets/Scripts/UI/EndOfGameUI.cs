using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndOfGameUI : MonoBehaviour
{ 
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Image levelSplash;
    [SerializeField] private TextMeshProUGUI highScoreText;

    private void Awake()
    {
        restartButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(2);

        });
        
        quitButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(0);
        });
    }

    private void Start()
    {
        GameManager.Instance.OnWinning += GameManager_OnWinning;
        Hide();
    }

    private void GameManager_OnWinning(object sender, EventArgs e)
    {
        Show();
        Cursor.lockState = CursorLockMode.None;
        highScoreText.text = "HIGH SCORE: " + PlayerPrefs.GetFloat("hiScore").ToString();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
