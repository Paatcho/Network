using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public readonly NetworkList<PlayerInfo> players = new();

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

        players.Add(new PlayerInfo(
            (int)clientId,
            $"Player {clientId}",
            cheese: 0,
            lives: 5
        ));
    }

    public void CollectCheese(int playerId)
    {
        if (!IsServer) return;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerId == playerId)
            {
                var p = players[i];
                p.cheese++;
                players[i] = p;
                break;
            }
        }
    }

    public void LoseLife(int playerId)
    {
        if (!IsServer) return;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerId == playerId)
            {
                var p = players[i];
                p.lives--;
                players[i] = p;
                break;
            }
        }
    }
}