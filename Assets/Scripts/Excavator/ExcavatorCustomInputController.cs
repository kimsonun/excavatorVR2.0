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
