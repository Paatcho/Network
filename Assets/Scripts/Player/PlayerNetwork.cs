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

    private readonly NetworkVariable<PlayerData> _playerData = new(
        new PlayerData
        {
            Exhausted = false,
            Direction = Vector3.forward,
            Velocity = 0f,
            Dead = false,
            
        },
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            print("burrrrr");
            controller.Init(this);
        }

        _playerData.OnValueChanged += (_, newValue) => { view.SetExhausted(newValue.Exhausted); };
    }

    private void Update()
    {
        view.UpdateDirection(_playerData.Value.Direction);
        view.UpdateView(_playerData.Value.Velocity);

        if (IsOwner)
        {
            controller.Move();

            if (Input.GetKeyDown(KeyCode.E))
            {
                EnterHole();
            }
        } 
    }

    [Rpc(SendTo.Everyone)]
    public void OnRespawnRpc()
    {
        view.OnRespawn();
    }
    
    public void Die(DeathType deathType = DeathType.Default)
    {
        if (!IsOwner) return;
        
        DieServerRpc(deathType);
    }

    [ServerRpc]
    private void DieServerRpc(DeathType deathType)
    {
        if (_playerData.Value.Dead) return;

        PlayerManager.Instance.LoseLife((int)OwnerClientId);

        DieClientRpc(deathType);

        _playerData.Value = new PlayerData { Dead = true };
    }
    
    [ClientRpc]
    private void DieClientRpc(DeathType deathType)
    {
        view.Die(deathType);

        if (IsOwner)
            controller.Die();
    }

    [Rpc(SendTo.Everyone)]
    public void PickUpCollectibleRpc(Collectible.CollectibleType collectibleType)
    {
        switch (collectibleType)
        {
            case Collectible.CollectibleType.Cheese:
                PlayerManager.Instance.CollectCheese((int)OwnerClientId);
                break;
            case Collectible.CollectibleType.Crumb:
                break;
        }
    }

    public void UpdatePlayerData(PlayerData playerData)
    {
        playerData.Direction =
            playerData.Direction == Vector3.zero ? _playerData.Value.Direction : playerData.Direction;
        _playerData.Value = playerData;
    }

    [Rpc(SendTo.Me)]
    public void TeleportRpc(Vector3 position)
    {
        networkTransform.Teleport(position, Quaternion.identity, Vector3.one);
    }

    private void EnterHole()
    {
        if (!controller.CurrentHole) return;
        
        controller.CurrentHole.Enter(controller);
    }
}

public struct PlayerData : INetworkSerializable
{
    public bool Exhausted;
    public Vector3 Direction;
    public float Velocity;
    public bool Dead;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Exhausted);
        serializer.SerializeValue(ref Direction);
        serializer.SerializeValue(ref Velocity);
        serializer.SerializeValue(ref Dead);
    }
}