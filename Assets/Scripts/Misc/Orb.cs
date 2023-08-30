using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private AudioClip collectSound;

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            player.onOrbNumberChange();
            GameManager.Instance.playSound(collectSound);
            Destroy(gameObject);
        }
    }
}
