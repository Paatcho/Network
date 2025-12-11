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
        Crumb
    }
    
    private CollectibleType _type;

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
                player.PickUpCollectible(network, _type);
                // Destroy(gameObject); // Destroy object on the client side.
            }
        }
    }
}
