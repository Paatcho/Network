using System;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum CollectibleType
    {
        Cheese,
        Crumbs
    }
    
    [SerializeField] private CollectibleType type;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerNetwork player = other.gameObject.GetComponent<PlayerNetwork>();
            player.PickUpCollectible(type);
            Destroy(gameObject);
        }
    }
}
