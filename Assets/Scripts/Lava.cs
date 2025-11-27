using System;
using UnityEngine;

public class Lava : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerNetwork player = other.GetComponent<PlayerNetwork>();
            
            player.Die(PlayerNetwork.DeathType.Explosion);
        }
    }
}
