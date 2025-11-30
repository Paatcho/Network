using System;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum CollectibleType
    {
        Cheese,
        Crumb
    }
    
    [SerializeField] private CollectibleType type;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<PlayerController>();
            player.PickUpCollectible(type);
            Destroy(gameObject);
        }
    }
}
