using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerListUI : MonoBehaviour
{
    [SerializeField] private PlayerCard playerCardPrefab;

    private readonly Dictionary<int, PlayerCard> _cards = new();

    public void OnNetworkSpawn()
    {
        PlayerManager.Instance.players.OnListChanged += OnPlayersChanged;
        
        RebuildUI();
    }

    public void OnNetworkDespawn()
    {
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.players.OnListChanged -= OnPlayersChanged;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RebuildUI();
        }
    }
    
    private void RebuildUI()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        _cards.Clear();
        
        foreach (var p in PlayerManager.Instance.players)
        {
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
        _cards[info.playerId] = card;
    }

    public void UpdateCard(PlayerInfo info)
    {
        if (_cards.TryGetValue(info.playerId, out var card))
        {
            card.UpdatePlayerCheeses(info.cheese);
            card.UpdatePlayerLives(info.lives);
        }
    }

    private void RemoveCard(int playerId)
    {
        if (_cards.TryGetValue(playerId, out var card))
        {
            Destroy(card.gameObject);
            _cards.Remove(playerId);
        }
    }
}
