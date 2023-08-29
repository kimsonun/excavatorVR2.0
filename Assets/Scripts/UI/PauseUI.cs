using AGXUnity_Excavator.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    public static PauseUI Instance { get; private set; }

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button themeButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI themeText;

    [SerializeField] private Slider trackSpeedSilder;
    [SerializeField] private TextMeshProUGUI trackSpeedValue;
    [SerializeField] private Slider bodySpeedSilder;
    [SerializeField] private TextMeshProUGUI bodySpeedValue;
    [SerializeField] private Slider boomSpeedSilder;
    [SerializeField] private TextMeshProUGUI boomSpeedValue;
    [SerializeField] private Slider armSpeedSilder;
    [SerializeField] private TextMeshProUGUI armSpeedValue;
    [SerializeField] private Slider bucketSpeedSilder;
    [SerializeField] private TextMeshProUGUI bucketSpeedValue;

    [SerializeField] private ExcavatorInputController controller;

    private void Awake()
    {
        Instance = this;
        resumeButton.onClick.AddListener(() =>
        {
           GameManager.Instance.TogglePauseGame();
        });
        themeButton.onClick.AddListener(() =>
        {
            GameManager.Instance.skyboxChange();
            UpdateVisual();
        });
        quitButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(0);
        });
    }

    private void Start()
    {
        controller = GameObject.Find("Excavator").GetComponent<ExcavatorInputController>();
        trackSpeedSilder.value = controller.rotateTrackSpeed;
        bodySpeedSilder.value = controller.rotateBodySpeed;
        boomSpeedSilder.value = controller.rotateBoomSpeed;
        armSpeedSilder.value = controller.rotateStickSpeed;
        bucketSpeedSilder.value = controller.rotateBucketSpeed;
        
        GameManager.Instance.OnGamePaused += GameManager_OnGamePaused;
        GameManager.Instance.OnGameUnpaused += GameManager_OnGameUnpaused;

        Hide();
    }

    private void GameManager_OnGamePaused(object sender, System.EventArgs e)
    {
        Show();
        
    }
    private void GameManager_OnGameUnpaused(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void UpdateVisual()
    {
        themeText.text = GameManager.Instance.getMaterialName();

    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void changeTrackSpeed()
    {
        trackSpeedValue.text = Mathf.RoundToInt(trackSpeedSilder.value).ToString();
        //ExcavatorController.Instance.setTrackRotateSpeed(float.Parse(trackSpeedValue.text));
        controller.rotateTrackSpeed = (float.Parse(trackSpeedValue.text));
    }
    public void changeBodySpeed()
    {
        bodySpeedValue.text = Mathf.RoundToInt(bodySpeedSilder.value).ToString();
        //ExcavatorController.Instance.setBodyRotateSpeed(float.Parse(bodySpeedValue.text));
        controller.rotateBodySpeed = (float.Parse(bodySpeedValue.text));
    }
    public void changeBoomSpeed()
    {
        boomSpeedValue.text = Mathf.RoundToInt(boomSpeedSilder.value).ToString();
        //ExcavatorController.Instance.setBoomRotateSpeed(float.Parse(boomSpeedValue.text));
        controller.rotateBoomSpeed = (float.Parse(boomSpeedValue.text));
    }
    public void changeArmSpeed()
    {
        armSpeedValue.text = Mathf.RoundToInt(armSpeedSilder.value).ToString();
        //ExcavatorController.Instance.setArmRotateSpeed(float.Parse(armSpeedValue.text));
        controller.rotateStickSpeed = (float.Parse(armSpeedValue.text));
    }
    public void changeBucketSpeed()
    {
        bucketSpeedValue.text = Mathf.RoundToInt(bucketSpeedSilder.value).ToString();
        //ExcavatorController.Instance.setBucketRotateSpeed(float.Parse(bucketSpeedValue.text));
        controller.rotateBucketSpeed = (float.Parse(bucketSpeedValue.text));
    }
}
