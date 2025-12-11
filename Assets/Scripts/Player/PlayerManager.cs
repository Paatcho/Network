using System.Collections.Generic;
using Unity.Netcode;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public readonly NetworkList<PlayerInfo> players = new(
        new List<PlayerInfo>(),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public readonly NetworkVariable<int> playersLeft = new();

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

        if (IsClient)
        {
            AddPlayerServerRpc();
        }
        
        CameraController.instance.playerListUI.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
        
        CameraController.instance.playerListUI.OnNetworkDespawn();
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerServerRpc()
    {
        playersLeft.Value++;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DecreasePlayerServerRpc()
    {
        playersLeft.Value--;
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

    public void PlayerLost()
    {
        DecreasePlayerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateCardServerRpc(int playerId, int lifeCount, int cheeseCount)
    {
        if (!IsServer) return;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerId == playerId)
            {
                var p = players[i];
                p.cheese = cheeseCount;
                p.lives = lifeCount;
                players[i] = p;
                UpdatePlayerCardClientRpc(p);
                return;
            }
        }
    }

    [ClientRpc]
    private void UpdatePlayerCardClientRpc(PlayerInfo p)
    {
        CameraController.instance.playerListUI.UpdateCard(p);
    }
}