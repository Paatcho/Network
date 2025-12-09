using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerListUI : NetworkBehaviour
{
    [SerializeField] private PlayerCard playerCardPrefab;

    private readonly Dictionary<int, PlayerCard> cards = new();

    public override void OnNetworkSpawn()
    {
        PlayerManager.Instance.Players.OnListChanged += OnPlayersChanged;
        
        print("list");

        RebuildUI();
    }

    public override void OnNetworkDespawn()
    {
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.Players.OnListChanged -= OnPlayersChanged;
    }

    private void RebuildUI()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        cards.Clear();
        
        print("Rebuilding UI");

        foreach (var p in PlayerManager.Instance.Players)
        {
            print(p.name);
            CreateCard(p);
        }
    }

    private void OnPlayersChanged(NetworkListEvent<PlayerInfo> change)
    {
        switch (change.Type)
        {
            case NetworkListEvent<PlayerInfo>.EventType.Add:
                CreateCard(change.Value);
                break;

            case NetworkListEvent<PlayerInfo>.EventType.Value:
                UpdateCard(change.Value);
                break;

            case NetworkListEvent<PlayerInfo>.EventType.Remove:
                RemoveCard(change.Value.playerId);
                break;
        }
    }

    private void CreateCard(PlayerInfo info)
    {
        var card = Instantiate(playerCardPrefab, transform);
        card.Init(info.name.ToString(), info.cheese, info.lives);
        cards[info.playerId] = card;
    }

    private void UpdateCard(PlayerInfo info)
    {
        if (cards.TryGetValue(info.playerId, out var card))
        {
            card.UpdatePlayerCheeses(info.cheese);
            card.UpdatePlayerLives(info.lives);
        }
    }

    private void RemoveCard(int playerId)
    {
        if (cards.TryGetValue(playerId, out var card))
        {
            Destroy(card.gameObject);
            cards.Remove(playerId);
        }
    }
}
