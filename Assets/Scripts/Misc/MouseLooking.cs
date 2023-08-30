using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLooking : MonoBehaviour
{
    public static MouseLooking instance;

    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private float minimumY = -20f;
    private float maximumY = 70f;
    public bool lockCursor = true;

    private void Awake()
    {
        instance = this;
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Start()
    {
        GameManager.Instance.OnGamePaused += GameManager_OnGamePaused;
        GameManager.Instance.OnGameUnpaused += GameManager_OnGameUnpaused;
    }

    private void GameManager_OnGameUnpaused(object sender, EventArgs e)
    { 
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void GameManager_OnGamePaused(object sender, EventArgs e)
    {
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        yaw += speedH * Input.GetAxis("Mouse X");
        pitch -= speedV * Input.GetAxis("Mouse Y");

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

        pitch = ClampAngle(pitch, minimumY, maximumY);
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
