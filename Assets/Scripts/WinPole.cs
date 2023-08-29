using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinPole : MonoBehaviour
{
    public Player player;
    [SerializeField] private int requireOrbToWin = 0;


    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (player.getOrb() >= requireOrbToWin)
        {
            GameManager.Instance.LevelComplete();
        }
    }
}
