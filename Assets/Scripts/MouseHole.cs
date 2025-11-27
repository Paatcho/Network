using System;
using System.Collections;
using Unity.Netcode.Components;
using UnityEngine;

public class MouseHole : MonoBehaviour
{
    [SerializeField] private MouseHole target;

    public void Enter(PlayerController player)
    {
        StartCoroutine(target.Exit(player));
    }
    
    private IEnumerator Exit(PlayerController player)
    {
        player.EnableController(false);
        player.Teleport(transform.position + transform.forward * 0.5f);
        yield return null;
        player.EnableController(true);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().CurrentHole = this;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().CurrentHole = null;
        }
    }
}
