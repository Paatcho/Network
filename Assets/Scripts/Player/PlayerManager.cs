using Unity.Netcode;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public NetworkList<PlayerInfo> Players;

    private void Awake()
    {
        Instance = this;
        Players = new NetworkList<PlayerInfo>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        var newPlayer = new PlayerInfo
        {
            playerId = (int)clientId,
            name = "Player " + clientId,
            cheese = 0,
            lives = 5
        };

        Players.Add(newPlayer);
    }

    public void AddCollectible(int playerId, Collectible.CollectibleType collectibleType)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].playerId == playerId)
            {
                PlayerInfo p = Players[i];

                switch (collectibleType)
                {
                    case Collectible.CollectibleType.Cheese:
                        p.cheese++;
                        break;
                }
                
                Players[i] = p;
                break;
            }
        }
    }

    public void LooseLife(int playerId)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].playerId == playerId)
            {
                PlayerInfo p = Players[i];
                
                p.lives--;
                
                Players[i] = p;
                break;
            }
        }
    }
}