using System;
using Unity.Netcode;
using UnityEngine;

public class HammerObstacle : NetworkBehaviour
{
    [SerializeField] private float hitInterval = 0.5f;
    [SerializeField] private Hammer hammer;

    private float _nextHitTime;

    private void Start()
    {
        _nextHitTime = Time.deltaTime;
    }

    private void Update()
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer) return;

        if (hammer.player) return;
        
        if (Time.time > _nextHitTime)
        {
            _nextHitTime = Time.time + hitInterval;
            hammer.HitServerRpc(false);
        }
    }
}
