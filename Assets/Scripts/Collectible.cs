using System;
using Unity.Netcode;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private NetworkObject network;
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

            if (player && player.enabled)
            {
                player.PickUpCollectible(network, _type);
                Destroy(gameObject); // Destroy object on the client side.
            }
        }
    }
}
