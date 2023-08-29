﻿using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Camera))]
public class FPSCamera : MonoBehaviour
{
  public float initialSpeed = 10f;
  public float increaseSpeed = 1.25f;

  public bool allowMovement = true;
  public bool allowRotation = true;

#if ENABLE_INPUT_SYSTEM
  InputAction m_forwardAction;
  InputAction m_backwardAction;
  InputAction m_rightAction;
  InputAction m_leftAction;
  InputAction m_cursorToggleAction;

  InputAction m_mouseXAction;
  InputAction m_mouseYAction;

#else
  public KeyCode forwardButton = KeyCode.W;
  public KeyCode backwardButton = KeyCode.S;
  public KeyCode rightButton = KeyCode.D;
  public KeyCode leftButton = KeyCode.A;
  public KeyCode cursorToggleButton = KeyCode.Escape;
#endif

  public float cursorSensitivity = 0.015f;
  public bool cursorToggleAllowed = true;

  private float currentSpeed = 0f;
  private bool moving = false;
  private bool togglePressed = false;

   
  private void OnEnable()
  {
    if (cursorToggleAllowed)
    {
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
    }
  }

  private void Start()
  {
#if ENABLE_INPUT_SYSTEM
    m_forwardAction = new InputAction("Forward", binding: "<Keyboard>/w");
    m_forwardAction.Enable();

    m_backwardAction = new InputAction("Backward", binding: "<Keyboard>/s");
    m_backwardAction.Enable();

    m_rightAction = new InputAction("Right", binding: "<Keyboard>/d");
    m_rightAction.Enable();

    m_leftAction = new InputAction("Left", binding: "<Keyboard>/a");
    m_leftAction.Enable();

    m_cursorToggleAction = new InputAction("Toggle", binding: "<Keyboard>/esc");
    m_cursorToggleAction.Enable();


    m_mouseXAction = new InputAction("mouseX", binding: "<Mouse>/delta");
    m_mouseXAction.Enable();

#endif

  }
  private void Update()
  {
    if (allowMovement)
    {
      bool lastMoving = moving;
      Vector3 deltaPosition = Vector3.zero;

      if (moving)
        currentSpeed += increaseSpeed * Time.deltaTime;

      moving = false;
#if ENABLE_INPUT_SYSTEM
      CheckMove(m_forwardAction.ReadValue<float>() > 0, ref deltaPosition, transform.forward);
      CheckMove(m_backwardAction.ReadValue<float>() > 0, ref deltaPosition, -transform.forward);
      CheckMove(m_rightAction.ReadValue<float>() > 0, ref deltaPosition, transform.right);
      CheckMove(m_leftAction.ReadValue<float>() > 0, ref deltaPosition, -transform.right);

#else
      CheckMove(Input.GetKey(forwardButton), ref deltaPosition, transform.forward );
      CheckMove(Input.GetKey(backwardButton), ref deltaPosition, -transform.forward );
      CheckMove(Input.GetKey(rightButton), ref deltaPosition, transform.right );
      CheckMove(Input.GetKey(leftButton), ref deltaPosition, -transform.right );
#endif
      if (moving)
      {
        if (moving != lastMoving)
          currentSpeed = initialSpeed;

        transform.position += deltaPosition * currentSpeed * Time.deltaTime;
      }
      else
        currentSpeed = 0f;
    }

    if (allowRotation)
    {
      Vector3 eulerAngles = transform.eulerAngles;
#if ENABLE_INPUT_SYSTEM
      var val = m_mouseXAction.ReadValue<Vector2>()* Time.deltaTime;
      eulerAngles.x += -val[1] * 359f * cursorSensitivity;
      eulerAngles.y += val[0] * 359f * cursorSensitivity;
#else

      eulerAngles.x += -Input.GetAxis( "Mouse Y" ) * 359f * cursorSensitivity;
      eulerAngles.y += Input.GetAxis( "Mouse X" ) * 359f * cursorSensitivity;
#endif
      transform.eulerAngles = eulerAngles;
    }

    if (cursorToggleAllowed)
    {
      bool doToggle = false;
#if ENABLE_INPUT_SYSTEM
      doToggle = m_cursorToggleAction.triggered;
#else
      doToggle = Input.GetKey( cursorToggleButton );
#endif

      if (doToggle)
      {
        if (!togglePressed)
        {
          togglePressed = true;
          if (Cursor.lockState == CursorLockMode.Locked)
            Cursor.lockState = CursorLockMode.Confined;
          else
            Cursor.lockState = CursorLockMode.Locked;
          Cursor.visible = !Cursor.visible;
        }
      }
      else
        togglePressed = false;
    }
    else
    {
      togglePressed = false;
      Cursor.visible = false;
    }
  }

  //private void CheckMove(KeyCode keyCode, ref Vector3 deltaPosition, Vector3 directionVector)
  private void CheckMove(bool doAction, ref Vector3 deltaPosition, Vector3 directionVector)
  {
    if (doAction)
    {
      moving = true;
      deltaPosition += directionVector;
    }
  }
}
