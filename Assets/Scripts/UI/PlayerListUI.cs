using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerListUI : MonoBehaviour
{
    [SerializeField] private PlayerCard playerCardPrefab;

    private readonly Dictionary<int, PlayerCard> _cards = new();

    private void Start()
    {
        //PlayerManager.Instance.Players.OnListChanged += OnPlayersChanged;
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
        _cards.Add(info.playerId, card);
    }

    private void UpdateCard(PlayerInfo info)
    {
        if (!_cards.TryGetValue(info.playerId, out var card)) return;

        card.UpdatePlayerCheeses(info.cheese);
        card.UpdatePlayerLives(info.lives);
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