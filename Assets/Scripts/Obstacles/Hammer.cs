using System;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class Hammer : NetworkBehaviour
{
    [SerializeField] private Vector3 hitTargetRotation;
    [SerializeField] private float hitTime = 0.1f;
    
    private bool _isHit = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HitServerRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    public void HitServerRpc()
    {
        if (_isHit) return;

        _isHit = true;
        transform.DORotate(hitTargetRotation, hitTime).SetEase(Ease.OutBounce)
            .OnComplete(() =>
            {
                _isHit = false;
                transform.DORotate(Vector3.zero, hitTime).SetEase(Ease.OutBack);
            });
    }

    public void Crush(PlayerNetwork player)
    {
        if (_isHit)
        {
            player.Die(PlayerNetwork.DeathType.Crushed);
        }
    }
}
