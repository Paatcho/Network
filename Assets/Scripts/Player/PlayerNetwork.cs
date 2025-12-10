using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    public enum DeathType
    {
        Default,
        Crushed,
        Explosion
    }
    
    [SerializeField] private PlayerController controller;
    [SerializeField] private PlayerView view;
    [SerializeField] private NetworkTransform networkTransform;

    public readonly NetworkVariable<bool> exhausted = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public readonly NetworkVariable<bool> dead = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public readonly NetworkVariable<PlayerMovementData> movementData = new(
        new PlayerMovementData
        {
            direction = Vector3.forward,
            velocity = 0f,
        },
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            controller.Init(this, networkTransform);
            view.Init(this);
        }
        else
        {
            controller.enabled = false;
        }

        exhausted.OnValueChanged += OnExhaustionChanged;
    }

    public override void OnNetworkDespawn()
    {
        exhausted.OnValueChanged -= OnExhaustionChanged;
    }

    private void OnExhaustionChanged(bool previous, bool current)
    {
        view.SetExhausted(current);
    }

    [ClientRpc]
    public void OnRespawnClientRpc()
    {
        view.OnRespawn();
    }
    
    public void Die(DeathType deathType = DeathType.Default)
    {
        if (!IsOwner || dead.Value) return;
        
        controller.Die();
        UpdatePlayerLifeServerRpc((int)OwnerClientId);
        DisplayDeathClientRpc(deathType);
        dead.Value = true;
    }

    [ServerRpc]
    private void UpdatePlayerLifeServerRpc(int ownerClientId)
    {
        PlayerManager.Instance.LoseLife(ownerClientId);
    }
    
    [ClientRpc]
    private void DisplayDeathClientRpc(DeathType deathType)
    {
        view.Die(deathType);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PickUpCollectibleServerRpc(NetworkObjectReference collectible, Collectible.CollectibleType type)
    {
        switch (type)
        {
            case Collectible.CollectibleType.Cheese:
                PlayerManager.Instance.CollectCheese((int)OwnerClientId);
                break;
            case Collectible.CollectibleType.Crumb:
                break;
        }
        
        //RequestCollectServerRpc(collectible);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestCollectServerRpc(NetworkObjectReference collectible)
    {
        if (collectible.TryGet(out NetworkObject obj))
        {
            obj.Despawn(true);
        }
        else
        {
            Debug.LogWarning("Collectible could not be found on server!");
        }
    }

    public void UpdateMovementData(PlayerMovementData newMovementData)
    {
        newMovementData.direction =
            newMovementData.direction == Vector3.zero ? movementData.Value.direction : newMovementData.direction;
        movementData.Value = newMovementData;
    }
}

public struct PlayerMovementData : INetworkSerializable
{
    public Vector3 direction;
    public float velocity;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref direction);
        serializer.SerializeValue(ref velocity);
    }
}