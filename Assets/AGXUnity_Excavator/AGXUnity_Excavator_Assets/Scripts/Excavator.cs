using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
namespace AGXUnity_Excavator.Editors
{
  [CustomEditor(typeof(AGXUnity_Excavator.Scripts.Excavator))]
  public class ExcavatorEditor : AGXUnityEditor.InspectorEditor
  { }
}
#endif


public static class TransformDeepChildExtension
{
  //Breadth-first search
  public static Transform FindDeepChild(this Transform aParent, string aName)
  {
    Queue<Transform> queue = new Queue<Transform>();
    queue.Enqueue(aParent);
    while (queue.Count > 0)
    {
      var c = queue.Dequeue();
      if (c.name == aName)
        return c;
      foreach (Transform t in c)
        queue.Enqueue(t);
    }
    return null;
  }
}


namespace AGXUnity_Excavator.Scripts
{

  using AGXUnity;
    using UnityEngine.SocialPlatforms.Impl;

#if UNITY_EDITOR
  public class ReadOnlyAttribute : PropertyAttribute { }
  /// <summary>
  /// This class contain custom drawer for ReadOnly attribute.
  /// </summary>
  [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
  public class ReadOnlyDrawer : PropertyDrawer
  {
    /// <summary>
    /// Unity method for drawing GUI in Editor
    /// </summary>
    /// <param name="position">Position.</param>
    /// <param name="property">Property.</param>
    /// <param name="label">Label.</param>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      // Saving previous GUI enabled value
      var previousGUIState = GUI.enabled;
      // Disabling edit for property
      GUI.enabled = false;
      // Drawing Property
      EditorGUI.PropertyField(position, property, label);
      // Setting old GUI enabled value
      GUI.enabled = previousGUIState;
    }
  }
#endif

  [AddComponentMenu("AGXUnity/Model/Excavator")]
  [DisallowMultipleComponent]
  public class Excavator : AGXUnity.ScriptComponent
  {
    public enum Location
    {
      Right,
      Left,
    }

    [SerializeField]
    private bool m_engineEnabled = true;

    [InspectorGroupBegin(Name = "Engine")]
    public bool EngineEnabled
    {
      get { return m_engineEnabled; }
      set
      {
        m_engineEnabled = value;
        if (Engine != null)
          Engine.setEnable(m_engineEnabled);
      }
    }

    
    [SerializeField]
    private float m_inletVolume = 0.029f; // Liter

    [ClampAboveZeroInInspector]
    public float InletVolume
    {
      get { return m_inletVolume; }
      set
      {
        m_inletVolume = value;
        if (Engine != null)
          Engine.setInletVolume(m_inletVolume);
      }
    }

    [SerializeField]
    private float m_volumetricEfficiency = 0.33f;

    [ClampAboveZeroInInspector]
    public float VolumetricEfficiency
    {
      get { return m_volumetricEfficiency; }
      set
      {
        m_volumetricEfficiency = value;
        if (Engine != null)
          Engine.setVolumetricEfficiency(m_volumetricEfficiency);
      }
    }


    [SerializeField]
    private float m_throttle = 0.0f;


    [FloatSliderInInspector(0.0f, 1.0f)]
    public float Throttle
    {
      get { return m_throttle; }
      set
      {
        m_throttle = value;
        if (Engine != null)
          Engine.setThrottle(m_throttle);
      }
    }

    [SerializeField]
    private float m_idleThrottleAngle = 0.17f;

    [ClampAboveZeroInInspector(true)]
    public float IdleThrottleAngle
    {
      get { return m_idleThrottleAngle; }
      set
      {
        m_idleThrottleAngle = Mathf.Min(value, 1.45f);
        if (Engine != null)
          Engine.setIdleThrottleAngle(m_idleThrottleAngle);
      }
    }

    [SerializeField]
    private float m_throttleBore = 0.3062f;

    [ClampAboveZeroInInspector]
    public float ThrottleBore
    {
      get { return m_throttleBore; }
      set
      {
        m_throttleBore = value;
        if (Engine != null)
          Engine.setThrottleBore(m_throttleBore);
      }
    }

    [SerializeField]
    private float m_numberOfRevolutionsPerCycle = 2.0f;

    [ClampAboveZeroInInspector]
    public float NumberOfRevolutionsPerCycle
    {
      get { return m_numberOfRevolutionsPerCycle; }
      set
      {
        m_numberOfRevolutionsPerCycle = Mathf.Max(value, 1.0f);
        if (Engine != null)
          Engine.setNrRevolutionsPerCycle(m_numberOfRevolutionsPerCycle);
      }
    }




#if UNITY_EDITOR

    [ReadOnly]
    public float EngineTorque = 0.0f;

    [ReadOnly]
    public float EnginePower = 0.0f;

    [ReadOnly]
    public float EngineRPM = 0.0f;

    [ReadOnly]
    public float ShaftRPM = 0.0f;
#endif

    private agxDriveTrain.Shaft m_shaft = null;

    
    [SerializeField]
    private float m_centralGearRatio = 550.0f;


    [InspectorGroupBegin(Name = "Gear Box")]
    public float CentralGearRatio
    {
      get { return m_centralGearRatio; }
      set
      {
        m_centralGearRatio = value;
        if (GearBox != null)
          GearBox.setGearRatio(m_centralGearRatio);
      }
    }


    private agx.Hinge[] m_brakeHinges = new agx.Hinge[] { null, null };
    private agxDriveTrain.Clutch[] m_clutches = new agxDriveTrain.Clutch[] { null, null };
    private agxDriveTrain.Gear[] m_gears = new agxDriveTrain.Gear[] { null, null };


    [InspectorGroupBegin(Name = "Clutches")]

    [SerializeField]
    private Vector2 m_clutchEfficiency = new Vector2(0, 0);

    public Vector2 ClutchEfficiency
    {
      get { return m_clutchEfficiency; }
      set
      {
        m_clutchEfficiency = value;
        if (LeftClutch != null)
          LeftClutch.setEfficiency(value[(int)Location.Left]);
        if (RightClutch != null)
          RightClutch.setEfficiency(value[(int)Location.Right]);
      }
    }


    [InspectorGroupBegin(Name = "Brakes")]

    [SerializeField]
    private Vector2 m_brakeEfficiency = new Vector2(0, 0);

    public Vector2 BrakeEfficiency
    {
      get { return m_brakeEfficiency; }
      set
      {

        // Simple model for brake. If value > 0, then apply brake
        m_brakeEfficiency = value;
        if (LeftBrake != null)
          LeftBrake.getMotor1D().setEnable(value[(int)Location.Left] > 0 ? true : false);

        if (RightBrake != null)
          RightBrake.getMotor1D().setEnable(value[(int)Location.Right] > 0 ? true : false);
      }
    }


    public agx.Hinge LeftBrake { get { return m_brakeHinges[(int)Location.Left]; } }
    public agx.Hinge RightBrake { get { return m_brakeHinges[(int)Location.Right]; } }
    

    public agxDriveTrain.Clutch LeftClutch { get { return m_clutches[(int)Location.Left]; } }
    public agxDriveTrain.Clutch RightClutch { get { return m_clutches[(int)Location.Right]; } }

    [InspectorGroupBegin(Name = "Gears")]
    [AllowRecursiveEditing]

    [SerializeField]
    private Vector2 m_gearRatio = new Vector2(1, 1);

    public Vector2 GearRatio
    {
      get { return m_gearRatio; }
      set
      {
        m_gearRatio = value;
        if (LeftGear != null)
          LeftGear.setGearRatio(value[(int)Location.Left]);

        if (RightGear != null)
          RightGear.setGearRatio(value[(int)Location.Right]);
      }
    }

    public agxDriveTrain.Gear LeftGear { get { return m_gears[(int)Location.Left]; } }
    public agxDriveTrain.Gear RightGear { get { return m_gears[(int)Location.Right]; } }

    [InspectorGroupBegin(Name = "Sprocket Hinges")]
    [AllowRecursiveEditing]
    public Constraint LeftHinge { get { return GetOrFindConstraint(Location.Left, "SprocketHinge", m_sprocketHinges); } }
    [AllowRecursiveEditing]
    public Constraint RightHinge { get { return GetOrFindConstraint(Location.Right, "SprocketHinge", m_sprocketHinges); } }

    [InspectorGroupBegin(Name = "Controlled Constraints")]
    [AllowRecursiveEditing]
    public Constraint SwingHinge
    {
      get
      {
        if (m_swingHinge == null)
          m_swingHinge = FindChild<Constraint>("CabinHinge");
        return m_swingHinge;
      }
    }

    [AllowRecursiveEditing]
    public Constraint LeftElevatePrismatic
    {
      get
      {
        if (m_boomPrismatics[(int)Location.Left] == null)
          m_boomPrismatics[(int)Location.Left] = FindChild<Constraint>("BoomPrismatic1");
        return m_boomPrismatics[(int)Location.Left];
      }
    }

    [AllowRecursiveEditing]
    public Constraint RightElevatePrismatic
    {
      get
      {
        if (m_boomPrismatics[(int)Location.Right] == null)
          m_boomPrismatics[(int)Location.Right] = FindChild<Constraint>("BoomPrismatic2");
        return m_boomPrismatics[(int)Location.Right];
      }
    }

    [AllowRecursiveEditing]
    public Constraint BucketPrismatic
    {
      get
      {
        if (m_bucketPrismatic == null)
          m_bucketPrismatic = FindChild<Constraint>("BucketPrismatic");
        return m_bucketPrismatic;
      }
    }

    [HideInInspector]
    public Constraint[] BoomPrismatics
    {
      get
      {
        if (m_boomPrismatics[0] == null || m_boomPrismatics[1] == null)
          return new Constraint[] { LeftElevatePrismatic, RightElevatePrismatic };
        return m_boomPrismatics;
      }
    }

    [AllowRecursiveEditing]
    public Constraint StickPrismatic
    {
      get
      {
        if (m_stickPrismatic == null)
          m_stickPrismatic = FindChild<Constraint>("ArmPrismatic");
        return m_stickPrismatic;
      }
    }

    [InspectorGroupEnd]
    [HideInInspector]
    public agxPowerLine.PowerLine PowerLine { get; private set; } = null;
    public agxDriveTrain.deprecatedCombustionEngine Engine { get; private set; } = null;
    public agxDriveTrain.Gear GearBox { get; private set; } = null;


    public float Speed
    {
      get
      {
        if (m_underCarriageObserver == null)
          m_underCarriageObserver = FindChild<ObserverFrame>("UnderCarriageObserver");
        if (m_underCarriageObserver == null || m_underCarriageObserver.Native == null)
          return 0.0f;

        var v = m_underCarriageObserver.Native.getVelocity();
        v = m_underCarriageObserver.Native.transformVectorToLocal(v);
        return (float)(3.6 * v[1]);
      }
    }

    public IEnumerable<Constraint> SprocketHinges
    {
      get
      {
        yield return LeftHinge;
        yield return RightHinge;
      }
    }

    protected override bool Initialize()
    {
      //
      // This is the whole drive train used                                                                                                                                                          
      //                                                                                                                                                           
      // engine -> engine_torque_converter_shaft -> central_torque_converter -> torque_converter_gear_shaft -> 
      //
      //
      //                                                                                          + -> central_gear_shaft -> clutch -> clutch_gear_shaft -> gear -> actuator_shaft -> actuator -> hinge
      //                                                                                          !
      // central_gear (550:1) -> central_gear_differential_shaft -> differential (locked) ->   +
      //                                                                                          !                                                                  
      //                                                                                          + -> central_gear_shaft -> clutch -> clutch_gear_shaft -> gear -> actuator_shaft -> actuator -> hinge
      //


      PowerLine = new agxPowerLine.PowerLine();
      PowerLine.setName(name);

      Engine = new agxDriveTrain.deprecatedCombustionEngine(InletVolume);
      Engine.setDischargeCoefficient( 0.015 );

      GearBox = new agxDriveTrain.Gear(m_centralGearRatio);

      m_actuators[(int)Location.Left] = new agxPowerLine.RotationalActuator(LeftHinge.GetInitialized<Constraint>().Native.asHinge());
      m_actuators[(int)Location.Right] = new agxPowerLine.RotationalActuator(RightHinge.GetInitialized<Constraint>().Native.asHinge());

      foreach (var sprocketHinge in SprocketHinges)
        sprocketHinge.GetController<TargetSpeedController>().Enable = false;


      var INPUT = agxPowerLine.Side.INPUT;
      var OUTPUT = agxPowerLine.Side.OUTPUT;

      PowerLine.add(Engine);

      GetSimulation().add(PowerLine);

      var central_torque_converter = new agxDriveTrain.TorqueConverter();
      var engine_torque_converter_shaft = new agxDriveTrain.Shaft();

      var differential = new agxDriveTrain.Differential();
      differential.setLock(true);
      var torque_converter_gear_shaft = new agxDriveTrain.Shaft();

      Engine.connect(OUTPUT, INPUT, torque_converter_gear_shaft);


      central_torque_converter.connect(agxPowerLine.Side.OUTPUT, INPUT, torque_converter_gear_shaft);
      GearBox.connect(agxPowerLine.Side.INPUT, OUTPUT, torque_converter_gear_shaft);

      var central_gear_differential_shaft = new agxDriveTrain.Shaft();
      GearBox.connect(agxPowerLine.Side.OUTPUT, INPUT, central_gear_differential_shaft);

      differential.connect(agxPowerLine.Side.INPUT, OUTPUT, central_gear_differential_shaft);

      // Connect the drive train to each sprocket hinge
      (m_clutches[(int)Location.Right], m_gears[(int)Location.Right], m_brakeHinges[(int)Location.Right]) = BuildDriveTrain(differential, m_actuators[(int)Location.Right], Location.Right);
      (m_clutches[(int)Location.Left], m_gears[(int)Location.Left], m_brakeHinges[(int)Location.Left]) = BuildDriveTrain(differential, m_actuators[(int)Location.Left], Location.Left);


      return true;
    }


    private (agxDriveTrain.Clutch, agxDriveTrain.Gear, agx.Hinge) BuildDriveTrain(agxDriveTrain.Differential differential, agxPowerLine.RotationalActuator actuator, Location location)
    {

      var differential_shaft = new agxDriveTrain.Shaft();

      differential.connect(agxPowerLine.Side.OUTPUT, agxPowerLine.Side.INPUT, differential_shaft);

      var clutch = new agxDriveTrain.Clutch();
      clutch.setEfficiency(0.0);
      clutch.connect(agxPowerLine.Side.INPUT, agxPowerLine.Side.OUTPUT, differential_shaft);

      var clutch_gear_shaft = new agxDriveTrain.Shaft();

      m_shaft = clutch_gear_shaft;


      clutch.connect(agxPowerLine.Side.OUTPUT, agxPowerLine.Side.INPUT, clutch_gear_shaft);

      var gear = new agxDriveTrain.Gear(m_gearRatio[(int)location]);
      clutch_gear_shaft.connect(gear);

      var actuator_shaft = new agxDriveTrain.Shaft();


      gear.connect(agxPowerLine.Side.OUTPUT, agxPowerLine.Side.INPUT, actuator_shaft);


      actuator.connect(actuator_shaft);

      var frame = new agx.Frame();
      var brake_body = actuator_shaft.getRotationalDimension().getOrReserveBody();
      frame.setLocalRotate(new agx.Quat(agx.Vec3.Z_AXIS(), actuator_shaft.getRotationalDimension().getWorldDirection()));


      var brake_hinge = new agx.Hinge(brake_body, frame);

      brake_hinge.getMotor1D().setEnable(true);
      brake_hinge.getMotor1D().setSpeed(0);
      brake_hinge.getMotor1D().setCompliance(1E-30);
      brake_hinge.getMotor1D().setForceRange(new agx.RangeReal(200000));


      GetSimulation().add(brake_hinge);

      return (clutch, gear, brake_hinge);
    }


    void Update()
    {
#if UNITY_EDITOR
      if (Engine != null)
      {
        EngineTorque = (float)Engine.getDeliveredTorque();
        EngineRPM = (float)Engine.getRPM();
        EnginePower = EngineTorque * EngineRPM / (9.5488f*1000.0f);

        ShaftRPM = (float)m_shaft.getRPM();
      }
#endif
      var f = Speed;
    }

    protected override void OnDestroy()
    {
      if (GetSimulation() != null)
      {
        GetSimulation().remove(PowerLine);
      }

      PowerLine = null;
      Engine = null;
      GearBox = null;
      m_actuators[(int)Location.Right] = null;
      m_actuators[(int)Location.Left] = null;

      base.OnDestroy();
    }

    private void Reset()
    {

    }

    /// <summary>
    /// Locate a named ScriptComponent in any child or sub-children of the component
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    private T FindChild<T>(string name)
      where T : ScriptComponent
    {
      var t = TransformDeepChildExtension.FindDeepChild(transform, name);
      return t.GetComponentInChildren<T>();
    }

    private Constraint GetOrFindConstraint(Location location, string postfix, Constraint[] cache)
    {
      if (cache[(int)location] == null)
        cache[(int)location] = FindChild<Constraint>(location.ToString() + postfix);
      return cache[(int)location];
    }


    private agxPowerLine.RotationalActuator[] m_actuators = new agxPowerLine.RotationalActuator[] { null, null };

    private ObserverFrame m_underCarriageObserver = null;
    private Constraint[] m_sprocketHinges = new Constraint[] { null, null };

    private Constraint m_swingHinge = null;
    private Constraint[] m_boomPrismatics = new Constraint[] { null, null };
    private Constraint m_stickPrismatic = null;
    private Constraint m_bucketPrismatic = null;

    }
} // namespace
