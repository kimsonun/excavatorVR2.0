using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ExcavatorCustomInputController : MonoBehaviour
{
    public static ExcavatorCustomInputController Instance { get; private set; }
    public InputDevice leftController;
    public InputDevice rightController;

    List<InputDevice> leftDevices = new List<InputDevice>();
    List<InputDevice> rightDevices = new List<InputDevice>();
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        StartCoroutine(GetDevices(1f));
    }
    private void Update()
    {
        
        if (leftController.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButtonValue) && primaryButtonValue)
        { 
            Debug.Log("pressing primary button( nut x)"); 
        }

        if (leftController.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerButtonValue) && triggerButtonValue)
        { 
            Debug.Log("pressing trigger button (nut o phia truoc)"); 
        }

        if (leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 primary2DAxis) && primary2DAxis != Vector2.zero)
        { 
            Debug.Log("moving joy stick"); 
        }
        if (leftController.TryGetFeatureValue(CommonUsages.gripButton, out bool gripButtonValue) && gripButtonValue)
        { 
            Debug.Log("pressing grip button (nut o ngang)"); 
        }
    }

    IEnumerator GetDevices(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        InputDeviceCharacteristics leftInputDeviceCharacteristics = InputDeviceCharacteristics.Left;
        InputDeviceCharacteristics rightInputDeviceCharacteristics = InputDeviceCharacteristics.Right;
        InputDevices.GetDevicesWithCharacteristics(leftInputDeviceCharacteristics, leftDevices);
        InputDevices.GetDevicesWithCharacteristics(rightInputDeviceCharacteristics, rightDevices);
        if (leftDevices.Count > 0 && rightDevices.Count > 0)
        {
            leftController = leftDevices[0];
            rightController = rightDevices[0];
        }
    }
}
