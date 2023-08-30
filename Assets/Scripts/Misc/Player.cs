using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private int orbsCount = 0;


    public int getOrb()
    {
        return orbsCount;
    }
    public int onOrbNumberChange()
    {
        return orbsCount++;
    }
}
