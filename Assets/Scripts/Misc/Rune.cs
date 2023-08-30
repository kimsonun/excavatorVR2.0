using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rune : MonoBehaviour
{
    public Player player;
    [SerializeField] private int requireOrbToWin = 0;
    private int count = 0;

    private void Start()
    {
        //player = GameObject.Find("Player").GetComponent<Player>();
        count = 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Player")
        {
            if (player.getOrb() >= requireOrbToWin)
            {
                if (count == 0)
                {
                    GameManager.Instance.LevelComplete();
                    count++;
                }
            }
        }
    }

    /*
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            if (player.getOrb() >= requireOrbToWin)
            {
                GameManager.Instance.LevelComplete();
            }
        }
    }
    */
}
