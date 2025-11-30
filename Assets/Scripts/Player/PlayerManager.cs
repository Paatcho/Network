using Unity.Netcode;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public NetworkList<PlayerInfo> players;

    private void Awake()
    {
        Instance = this;
        players = new NetworkList<PlayerInfo>();
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

        players.Add(newPlayer);
    }

    public void AddCollectible(int playerId, Collectible.CollectibleType collectibleType)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerId == playerId)
            {
                PlayerInfo p = players[i];

                switch (collectibleType)
                {
                    case Collectible.CollectibleType.Cheese:
                        p.cheese++;
                        break;
                }
                
                players[i] = p;
                break;
            }
        }
    }

    public void LooseLife(int playerId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerId == playerId)
            {
                PlayerInfo p = players[i];
                
                p.lives--;
                
                players[i] = p;
                break;
            }
        }
    }
}