using System;
using System.Collections.Generic;
using System.Linq;
using AGXUnity;

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


#if UNITY_EDITOR
using UnityEditor;
namespace AGXUnity_GraspingRobot.Editors
{
  [CustomEditor(typeof(AGXUnity_Excavator.Scripts.ExcavatorInputController))]
  public class ExcavatorInputControllerEditor : AGXUnityEditor.InspectorEditor
  { }
}
#endif


namespace AGXUnity_Excavator.Scripts
{
  [AddComponentMenu("AGXUnity_Excavator.Scripts/Excavator Input Controller")]
  public class ExcavatorInputController : ScriptComponent
  {

        [SerializeField]
    public float MaxRotationalAcceleration = 1; // rad/s^2

    [SerializeField]
    public float MaxLinearAcceleration = 0.3f; // m/s^2
    
    public float rotateTrackSpeed = 1f;
    public float rotateBodySpeed = 1f;
    public float rotateBoomSpeed = 1f;
    public float rotateStickSpeed = 1f;
    public float rotateBucketSpeed = 1f;
        private Vector2 input;

        public enum ActionType
    {
      Bucket,
      Boom,
      Stick,
      Swing,
      Steer,
      Drive,
      CustomBucket,
      CustomBoom,
      CustomStick,
      CustomSwing,
    }

    [HideInInspector]
    public Excavator Excavator
    {
      get
      {
        if ( m_excavator == null )
          m_excavator = GetComponent<Excavator>();
        return m_excavator;
      }
    }

#if ENABLE_INPUT_SYSTEM
    [SerializeField]
    private InputActionAsset m_inputAsset = null;

    public InputActionAsset InputAsset
    {
      get
      {
        return m_inputAsset;
      }
      set
      {
        m_inputAsset = value;
        InputMap = m_inputAsset?.FindActionMap( "Excavator" );

        if ( InputMap != null && IsSynchronizingProperties ) {
          m_hasValidInputActionMap = true;
          foreach ( var actionName in System.Enum.GetNames( typeof( ActionType ) ) ) {
            if ( InputMap.FindAction( actionName ) == null ) {
              Debug.LogWarning( $"Unable to find Input Action: Excavator.{actionName}" );
              m_hasValidInputActionMap = false;
            }
          }

          if ( m_hasValidInputActionMap )
            InputMap.Enable();
          else
            Debug.LogWarning( "Excavator input disabled due to missing action(s) in the action map." );
        }

        if ( m_inputAsset != null && InputMap == null )
          Debug.LogWarning( "InputActionAsset doesn't contain an ActionMap named \"Excavator\"." );
      }
    }

    private InputAction SaveAction;

    public InputActionMap InputMap = null;
#endif

    [HideInInspector]
    public float Steer { get { return GetValue( ActionType.Steer ); } }

    [HideInInspector]
    public float Boom { get { return GetValue( ActionType.Boom ); } }

    [HideInInspector]
    public float Bucket { get { return GetValue( ActionType.Bucket ); } }

    [HideInInspector]
    public float Stick { get { return GetValue(ActionType.Stick); } }

    [HideInInspector]
    public float Swing { get { return GetValue(ActionType.Swing); } }

    [HideInInspector]
    public float Drive { get { return GetValue(ActionType.Drive); } }

    public Vector2 CustomBucket{ get { return GetVector2Xaxis( ActionType.CustomBucket ); } }
    public Vector2 CustomBoom{ get { return GetVector2Yaxis( ActionType.CustomBoom ); } }
    public Vector2 CustomSwing{ get { return GetVector2Xaxis( ActionType.CustomSwing ); } }
    public Vector2 CustomStick{ get { return GetVector2Yaxis( ActionType.CustomStick ); } }

    public float GetValue( ActionType action )
    {
#if ENABLE_INPUT_SYSTEM
      return m_hasValidInputActionMap ? InputMap[ action.ToString() ].ReadValue<float>() : 0.0f;
#else
      var name = action.ToString();
      var jAction = Input.GetAxis( 'j' + name );
      return jAction != 0.0f ? jAction : Input.GetAxis( 'k' + name );
#endif
    }
        public Vector2 GetVector2Xaxis(ActionType action)
        {
            input = m_hasValidInputActionMap ? InputMap[action.ToString() ].ReadValue<Vector2>() : Vector2.zero;
            return new Vector2(input.x, 0);
        }
        public Vector2 GetVector2Yaxis(ActionType action)
        {
            input = m_hasValidInputActionMap ? InputMap[action.ToString() ].ReadValue<Vector2>() : Vector2.zero;
            return new Vector2(0, input.y);
        }

    protected override bool Initialize()
    {
      if ( Excavator == null ) {
        Debug.LogError( "Unable to initialize: AGXUnity.Model.Excavator component not found.", this );
        return false;
      }

      SaveAction = new InputAction("Save", binding: "<Keyboard>/o");
      SaveAction.Enable();
   
      return true;
    }

    private void Reset()
    {
#if ENABLE_INPUT_SYSTEM
      InputAsset = Resources.Load<InputActionAsset>( "Input/AGXUnityInputControls" );
#endif
    }

    private void Update()
    {

#if ENABLE_INPUT_SYSTEM
      if (SaveAction.triggered)
      {
        GetSimulation().write("agxunity_simulation.agx");
      }
#endif
      var t = Time.fixedTime;

      var speed = Excavator.Speed;

      var steer = Steer * rotateTrackSpeed;
      var drive = Drive;

      // Increase throttle only when we drive or turn
      float throttle = Mathf.Abs(steer) + Mathf.Abs(drive) > 0 ? 1 : 0;
      SetThrottle(throttle);

      if (Mathf.Abs(throttle) > 0)
      {
        Excavator.ClutchEfficiency = new Vector2(1, 1);
        Excavator.BrakeEfficiency = new Vector2(0, 0);
      }
      else
      {
        Excavator.ClutchEfficiency = new Vector2(0, 0);
        Excavator.BrakeEfficiency = new Vector2(1, 1);
      }

      Vector2 gear = new Vector2(0, 0);
      Excavator.GearRatio = gear;

      if (Mathf.Abs(drive) > 0)
      {
        gear[0] = -Mathf.Sign(drive);
        gear[1] = -Mathf.Sign(drive);
      }

      var gear_ratio = 1.0f; // When turning
      if (AGXUnity.Utils.Math.EqualsZero(steer))
        gear_ratio = 0.2f; // When driving forward/backward

      if (Mathf.Abs(steer) > 0)
      {
        gear[0] = -Mathf.Sign(steer);
        gear[1] = Mathf.Sign(steer);
      }

      Excavator.GearRatio = gear * gear_ratio;

      SetBoom(CustomBoom.y * 0.3f * rotateBoomSpeed);
      SetBucket(CustomBucket.x * 0.7f * rotateBucketSpeed);
      SetStick(-CustomStick.y * 0.7f * rotateStickSpeed);


      var value = CustomSwing.x*0.6f;
      if (Mathf.Abs(value) < 0.3)
        value = 0;

      SetSwing(value * rotateBodySpeed);

    }

    private void SetThrottle( float value )
    {
      Excavator.Throttle =  value;
    }
  
    private void SetSwing(float value)
    {
      float currentSpeed = (float)Excavator.SwingHinge.Native.asHinge().getCurrentSpeed();
      var newSpeed = CalculateSpeed(value, currentSpeed, MaxRotationalAcceleration);

      SetSpeed(Excavator.SwingHinge, newSpeed);
    }

    private void SetBoom(float value)
    {
      float currentSpeed = (float)Excavator.BoomPrismatics[0].Native.asPrismatic().getCurrentSpeed();
      var newSpeed = CalculateSpeed(value, currentSpeed, MaxLinearAcceleration);

      foreach ( var prismatic in Excavator.BoomPrismatics )
        SetSpeed( prismatic, newSpeed);
    }

    float CalculateSpeed(float desiredSpeed, float currentSpeed, float maxAcceleration)
    {
      float dt = (float)GetSimulation().getTimeStep();
      float maxDeltaSpeed = Mathf.Abs((float)(maxAcceleration * dt));
      float maxSpeed = maxDeltaSpeed + Mathf.Abs(currentSpeed);

      float newSpeed = Mathf.Clamp(desiredSpeed, currentSpeed - maxDeltaSpeed, currentSpeed + maxDeltaSpeed);
      return newSpeed;
    }
    private void SetStick( float value )
    {
      float currentSpeed = (float)Excavator.StickPrismatic.Native.asPrismatic().getCurrentSpeed();
      var newSpeed = CalculateSpeed(value, currentSpeed, MaxLinearAcceleration);

      SetSpeed(Excavator.StickPrismatic, newSpeed);
    }

    private void SetBucket(float value)
    {
      float currentSpeed = (float)Excavator.BucketPrismatic.Native.asPrismatic().getCurrentSpeed();
      var newSpeed = CalculateSpeed(value, currentSpeed, MaxLinearAcceleration);

      SetSpeed(Excavator.BucketPrismatic, newSpeed);
    }

    private void SetSpeed( Constraint constraint, float speed )
    {
      var motorEnable = !AGXUnity.Utils.Math.EqualsZero( speed );
      var mc = constraint.GetController<TargetSpeedController>();
      var lc = constraint.GetController<LockController>();
      mc.Enable = true;

      if (AGXUnity.Utils.Math.EqualsZero(speed))
        speed = 0;

      mc.Speed = speed;
      //if ( !motorEnable && !lc.Enable )
      //  lc.Position = constraint.GetCurrentAngle();
      lc.Enable = false;
    }

    private Excavator m_excavator = null;
#if ENABLE_INPUT_SYSTEM
    private bool m_hasValidInputActionMap = false;
#endif
  }
}

#region InputManager.asset
  /*
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!13 &1
InputManager:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Axes:
  - serializedVersion: 3
    m_Name: jSteer
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: 
    altNegativeButton: left
    altPositiveButton: right
    gravity: 3
    dead: 0.3
    sensitivity: 1
    snap: 1
    invert: 0
    type: 2
    axis: 0
    joyNum: 0
  - serializedVersion: 3
    m_Name: kSteer
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: left
    positiveButton: right
    altNegativeButton: 
    altPositiveButton: 
    gravity: 3
    dead: 0.001
    sensitivity: 2
    snap: 1
    invert: 0
    type: 0
    axis: 0
    joyNum: 0
  - serializedVersion: 3
    m_Name: jThrottle
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: 
    altNegativeButton: 
    altPositiveButton: 
    gravity: 3
    dead: 0.05
    sensitivity: 1
    snap: 0
    invert: 0
    type: 2
    axis: 9
    joyNum: 0
  - serializedVersion: 3
    m_Name: kThrottle
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: up
    altNegativeButton: 
    altPositiveButton: 
    gravity: 3
    dead: 0.001
    sensitivity: 2
    snap: 0
    invert: 0
    type: 0
    axis: 0
    joyNum: 0
  - serializedVersion: 3
    m_Name: jBrake
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: 
    altNegativeButton: 
    altPositiveButton: 
    gravity: 3
    dead: 0.05
    sensitivity: 1
    snap: 0
    invert: 0
    type: 2
    axis: 8
    joyNum: 0
  - serializedVersion: 3
    m_Name: kBrake
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: down
    altNegativeButton: 
    altPositiveButton: 
    gravity: 3
    dead: 0.001
    sensitivity: 2
    snap: 0
    invert: 0
    type: 0
    axis: 0
    joyNum: 0
  - serializedVersion: 3
    m_Name: jElevate
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: 
    altNegativeButton: 
    altPositiveButton: 
    gravity: 3
    dead: 0.3
    sensitivity: 1
    snap: 0
    invert: 1
    type: 2
    axis: 1
    joyNum: 0
  - serializedVersion: 3
    m_Name: kElevate
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: s
    positiveButton: w
    altNegativeButton: 
    altPositiveButton: 
    gravity: 3
    dead: 0.001
    sensitivity: 1
    snap: 0
    invert: 0
    type: 0
    axis: 0
    joyNum: 0
  - serializedVersion: 3
    m_Name: jTilt
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: 
    altNegativeButton: 
    altPositiveButton: 
    gravity: 3
    dead: 0.3
    sensitivity: 1
    snap: 0
    invert: 0
    type: 2
    axis: 3
    joyNum: 0
  - serializedVersion: 3
    m_Name: kTilt
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: a
    positiveButton: d
    altNegativeButton: 
    altPositiveButton: 
    gravity: 3
    dead: 0.001
    sensitivity: 1
    snap: 0
    invert: 0
    type: 0
    axis: 0
    joyNum: 0
  - serializedVersion: 3
    m_Name: Enable Debug Button 1
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: left ctrl
    altNegativeButton: 
    altPositiveButton: joystick button 8
    gravity: 0
    dead: 0
    sensitivity: 0
    snap: 0
    invert: 0
    type: 0
    axis: 0
    joyNum: 0
  - serializedVersion: 3
    m_Name: Enable Debug Button 2
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: backspace
    altNegativeButton: 
    altPositiveButton: joystick button 9
    gravity: 0
    dead: 0
    sensitivity: 0
    snap: 0
    invert: 0
    type: 0
    axis: 0
    joyNum: 0
  - serializedVersion: 3
    m_Name: Debug Reset
    descriptiveName: 
    descriptiveNegativeName: 
    negativeButton: 
    positiveButton: left alt
    altNegativeButton: 
    altPositiveButton: joystick button 1
    gravity: 0
    dead: 0
    sensitivity: 0
    snap: 0
    invert: 0
    type: 0
    axis: 0
    joyNum: 0
    */
#endregion
  