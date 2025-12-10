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

    public const int CheeseWinCount = 1;
    
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
    
    public readonly NetworkVariable<bool> lost = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    
    public readonly NetworkVariable<bool> win = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    
    private int _cheeseCount = 0;
    private int _lifeCount = 5;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            controller.Init(this, networkTransform);
        }
        else
        {
            controller.enabled = false;
        }

        view.Init(this);
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

    [ServerRpc(RequireOwnership = false)]
    public void OnRespawnServerRpc()
    {
        OnRespawnClientRpc();
    }
    
    [ClientRpc]
    private void OnRespawnClientRpc()
    {
        view.OnRespawn();
    }
    
    public void Die(DeathType deathType = DeathType.Default)
    {
        if (!IsOwner || dead.Value) return;
        
        _cheeseCount = 0;
        _lifeCount--;
        controller.Die(_lifeCount);

        DisplayDeathServerRpc(deathType);
        print("www");
        UpdatePlayerLifeServerRpc();
        dead.Value = true;
        
        if (_lifeCount <= 0)
        {
            OnLost();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerLifeServerRpc()
    {
        PlayerManager.Instance.UpdateCardServerRpc((int)OwnerClientId, _lifeCount, _cheeseCount);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DisplayDeathServerRpc(DeathType deathType)
    {
        DisplayDeathClientRpc(deathType);
    }
    
    [ClientRpc]
    private void DisplayDeathClientRpc(DeathType deathType)
    {
        print("sss");
        view.Die(deathType);
    }

    public void PickUpCollectible(NetworkObjectReference collectible, Collectible.CollectibleType type)
    {
        switch (type)
        {
            case Collectible.CollectibleType.Cheese:
                _cheeseCount++;
                PickUpCheeseServerRpc(collectible);
                if (_cheeseCount == CheeseWinCount)
                {
                    WinServerRpc();
                }
                break;
            case Collectible.CollectibleType.Crumb:
                break;
        }
    }

    [ServerRpc]
    private void PickUpCheeseServerRpc(NetworkObjectReference collectible)
    {
        PlayerManager.Instance.UpdateCardServerRpc((int)OwnerClientId, _lifeCount, _cheeseCount);
        RequestCollectServerRpc(collectible);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestCollectServerRpc(NetworkObjectReference collectible)
    {
        if (collectible.TryGet(out NetworkObject obj))
        {
            obj.Despawn();
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

    [ServerRpc(RequireOwnership = false)]
    public void WinServerRpc()
    {
        WinClientRpc();
    }
    
    [ClientRpc]
    private void WinClientRpc()
    {
        if (IsOwner)
        {
            win.Value = true;
            controller.ResetSize();
        }
        
        view.LaunchWinAnimation();

        if (!win.Value)
        {
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerNetwork>().OnLost();
        }
    }

    private void OnLost()
    {
        // Dead :c
        if (!lost.Value)
        {
            CameraController.instance.SetTitle("You lost.");
            CameraController.instance.SetSubtitle("All the cheese has been eaten.");
            
            lost.Value = true;
            controller.CurrentMode = PlayerController.PlayerMode.Spectate;
            controller.ChangeSpectatePlayer(1);
            
            PlayerManager.Instance.PlayerLost();
        }
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