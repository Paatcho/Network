using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour, IPlayerDataListener
{
    [SerializeField] private NetworkObject prefab;
    [SerializeField] private PlayerController controller;
    [SerializeField] private PlayerView view;

    private readonly NetworkVariable<PlayerData> _playerData = new(
        new PlayerData
        {
            Exhausted = false,
            Direction = Vector3.forward,
            Velocity = 0f
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

    public void UpdatePlayerData(PlayerData playerData)
    {
        playerData.Direction =
            playerData.Direction == Vector3.zero ? _playerData.Value.Direction : playerData.Direction;
        _playerData.Value = playerData;
    }
}

public struct PlayerData : INetworkSerializable
{
    public bool Exhausted;
    public Vector3 Direction;
    public float Velocity;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Exhausted);
        serializer.SerializeValue(ref Direction);
        serializer.SerializeValue(ref Velocity);
    }
}

public interface IPlayerDataListener
{
    public void UpdatePlayerData(PlayerData playerData);
}