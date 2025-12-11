using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private NetworkObject network;
    [SerializeField] private SpawnedElement spawnedElement;
    
    public enum CollectibleType
    {
        Cheese,
        Hammer
    }
    
    [SerializeField] private CollectibleType type;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<PlayerController>();

            if (spawnedElement)
            {
                spawnedElement.OnCollected();
            }

            if (player && player.enabled)
            {
                player.PickUpCollectible(network, type);
            }
        }
    }
}
