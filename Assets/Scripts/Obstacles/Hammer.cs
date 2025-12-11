using System;
using System.Collections;
using DG.Tweening;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Hammer : NetworkBehaviour
{
    [SerializeField] private Vector3 hitTargetRotation;
    [SerializeField] private float hitTime = 0.1f;
    [SerializeField] private GameObject model;
    [SerializeField] private Transform hammer;
    
    private bool _isHit = false;

    public PlayerNetwork player;

    [Rpc(SendTo.Everyone)]
    public void HitServerRpc(bool isPlayer)
    {
        if (_isHit) return;
        
        _isHit = true;
        
        if (!isPlayer)
        {
            hammer.DORotate(hitTargetRotation, hitTime).SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    _isHit = false;
                    hammer.DORotate(Vector3.zero, hitTime).SetEase(Ease.OutBack);
                });
        }
        else
        {
            float randomY = UnityEngine.Random.Range(0f, 360f);
            Vector3 randomRot = new Vector3(hitTargetRotation.x, randomY, hitTargetRotation.z);

            hammer.DORotate(randomRot, hitTime)
                .SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    _isHit = false;
                    hammer.DORotate(Vector3.zero, hitTime)
                        .SetEase(Ease.OutBack);
                });

        }
    }

    public void Crush(PlayerNetwork player)
    {
        if (_isHit && this.player != player)
        {
            player.Die(PlayerNetwork.DeathType.Crushed);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AttachToPlayerServerRpc(NetworkObjectReference player)
    {
        if (player.TryGet(out NetworkObject obj))
        {
            this.player = obj.GetComponent<PlayerNetwork>();
            
            // NGO-approved parenting
            NetworkObject.TrySetParent(obj);

            // Set local offset AFTER parenting
            transform.localPosition = Vector3.up * 1f;
            transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning("Player could not be found on server!");
        }

        StartCoroutine(DisappearCoroutine());
    }
    
    private IEnumerator DisappearCoroutine()
    {
        yield return new WaitForSeconds(10f);
        
        for (int i = 0; i < 10; i++)
        {
            model.SetActive(model.activeInHierarchy);
            yield return new WaitForSeconds(0.1f);
        }
        
        player.controller.hammer = null;
        DestroyHammerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyHammerServerRpc()
    {
        // Destroy(gameObject);
        NetworkObject.Despawn();
    }
}
