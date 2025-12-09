using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public NetworkList<PlayerInfo> Players = new();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        Players.Add(new PlayerInfo(
            (int)clientId,
            $"Player {clientId}",
            cheese: 0,
            lives: 5
        ));
    }

    public void CollectCheese(int playerId)
    {
        if (!IsServer) return;

        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].playerId == playerId)
            {
                var p = Players[i];
                p.cheese++;
                Players[i] = p;
                break;
            }
        }
    }

    public void LoseLife(int playerId)
    {
        if (!IsServer) return;

        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].playerId == playerId)
            {
                var p = Players[i];
                p.lives--;
                Players[i] = p;
                break;
            }
        }
    }
}