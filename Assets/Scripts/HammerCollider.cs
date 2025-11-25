using System;
using UnityEngine;
using UnityEngine.Events;

public class HammerCollider : MonoBehaviour
{
    [SerializeField] private Hammer hammer;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerNetwork>();
            hammer.Crush(player);
        }
    }
}
