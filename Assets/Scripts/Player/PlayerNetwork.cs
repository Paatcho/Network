using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    public enum DeathType
    {
        Default,
        Crushed
    }
    
    [SerializeField] private NetworkObject prefab;
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
        } 
    }

    private NetworkObject _spawnedObject;

    [Rpc(SendTo.Server)]
    private void DestroyObjectRpc()
    {
        _spawnedObject.Despawn();
    }

    [Rpc(SendTo.Server)]
    private void TestRpc()
    {
        _spawnedObject = Instantiate(prefab, transform.position + new Vector3(0, Random.Range(2, 8), 0),
            Quaternion.identity);
        _spawnedObject.Spawn(true);
    }

    [Rpc(SendTo.Everyone)]
    public void OnRespawnRpc()
    {
        view.OnRespawn();
    }
    
    public void OnCrushed()
    {
        if (!IsOwner) return;
        
        DieRpc(DeathType.Crushed);
    }

    [Rpc(SendTo.Everyone)]
    private void DieRpc(DeathType deathType = DeathType.Default)
    {
        if (_playerData.Value.Dead) return;

        view.Die(deathType);
        if (IsOwner) controller.Die();
    }

    public void UpdatePlayerData(PlayerData playerData)
    {
        playerData.Direction =
            playerData.Direction == Vector3.zero ? _playerData.Value.Direction : playerData.Direction;
        _playerData.Value = playerData;
    }

    [Rpc(SendTo.Everyone)]
    public void TeleportRpc(Vector3 position)
    {
        print(position);
        networkTransform.Teleport(position, Quaternion.identity, Vector3.one);
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