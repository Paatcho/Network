using System;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class MovingFloor : NetworkBehaviour
{
    [SerializeField] private Transform floor;
    [SerializeField] private Vector3 pos1;
    [SerializeField] private Vector3 pos2;
    [SerializeField] private float moveTime;

    private bool _target = false;
    private bool _moving = false;
    
    private void Update()
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer) return;

        if (!_moving)
        {
            GoToNextPosRpc();
        }
    }
    
    [Rpc(SendTo.Everyone)]
    private void GoToNextPosRpc()
    {
        _moving = true;
        
        floor.DOLocalMove(_target ? pos1 : pos2, moveTime).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                _moving = false;
                _target = !_target;
            });
    }
}
