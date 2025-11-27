using System;
using UnityEngine;

public class CrushTrigger : MonoBehaviour
{
    [SerializeField] private PlayerNetwork player;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (rb && rb.linearVelocity.y < 0f)
            {
                player.Die(PlayerNetwork.DeathType.Crushed);
            }
        }
    }
}
