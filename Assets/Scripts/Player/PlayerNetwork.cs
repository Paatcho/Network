using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    public enum DeathType
    {
        Default,
        Crushed,
        Explosion
    }
    
    public enum WinType
    {
        Cheese,
        Death
    }

    public const int CheeseWinCount = 10;
    
    [SerializeField] private NetworkTransform networkTransform;
    [SerializeField] private NetworkObject cheesePrefab;
    [SerializeField] private NetworkObject hammerPrefab;
    public PlayerController controller;
    public PlayerView view;

    public readonly NetworkVariable<bool> exhausted = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public readonly NetworkVariable<bool> dead = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public readonly NetworkVariable<PlayerMovementData> movementData = new(
        new PlayerMovementData
        {
            direction = Vector3.forward,
            velocity = 0f,
        },
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    
    public readonly NetworkVariable<bool> lost = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    
    public readonly NetworkVariable<bool> win = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    
    public readonly NetworkVariable<int> winAnimIndex = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    
    private int _cheeseCount = 0;
    private int _lifeCount = 3;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            controller.Init(this, networkTransform);
            CameraController.instance.SetLobbyUI(false);
            winAnimIndex.Value = Random.Range(0, view.AnimCount);
        }
        else
        {
            controller.enabled = false;
        }

        view.Init(this);
        exhausted.OnValueChanged += OnExhaustionChanged;
    }

    public override void OnNetworkDespawn()
    {
        exhausted.OnValueChanged -= OnExhaustionChanged;
    }

    private void OnExhaustionChanged(bool previous, bool current)
    {
        view.SetExhausted(current);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnRespawnServerRpc()
    {
        OnRespawnClientRpc();
    }
    
    [ClientRpc]
    private void OnRespawnClientRpc()
    {
        view.OnRespawn();
    }
    
    public void Die(DeathType deathType = DeathType.Default)
    {
        if (!IsOwner || dead.Value) return;

        if (deathType != DeathType.Explosion)
        {
            SpawnCheeseOnDeathServerRpc((int)(_cheeseCount / 1.5f));
        }
        
        _cheeseCount = 0;
        _lifeCount--;
        controller.Die(_lifeCount);

        DisplayDeathServerRpc(deathType);
        UpdatePlayerLifeServerRpc(_lifeCount, _cheeseCount);
        dead.Value = true;
        
        if (_lifeCount <= 0)
        {
            OnLost();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerLifeServerRpc(int lifeCount, int cheeseCount)
    {
        PlayerManager.Instance.UpdateCardServerRpc((int)OwnerClientId, lifeCount, cheeseCount);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DisplayDeathServerRpc(DeathType deathType)
    {
        DisplayDeathClientRpc(deathType);
    }

    private void CheckForWinner()
    {
        int aliveCount = 0;
        
        foreach (NetworkClient client in NetworkManager.ConnectedClients.Values)
        {
            if (!client.PlayerObject.GetComponent<PlayerNetwork>().lost.Value)
            {
                aliveCount++;
            }
        }
        
        if (aliveCount <= 1)
        {
            CallForWinServerRpc();
        }
    }

    [ServerRpc]
    private void CallForWinServerRpc()
    {
        foreach (NetworkClient client in NetworkManager.ConnectedClients.Values)
        {
            client.PlayerObject.GetComponent<PlayerNetwork>().CallForWinClientRpc();
        }
    }

    [ClientRpc]
    private void CallForWinClientRpc()
    {
        if (!IsOwner) return;

        if (!lost.Value && !win.Value)
        {
            WinServerRpc(WinType.Death);
        }
    }
    
    [ClientRpc]
    private void DisplayDeathClientRpc(DeathType deathType)
    {
        view.Die(deathType);
    }

    public void PickUpCollectible(NetworkObjectReference collectible, Collectible.CollectibleType type)
    {
        switch (type)
        {
            case Collectible.CollectibleType.Cheese:
                _cheeseCount++;
                PickUpCheeseServerRpc(collectible, _lifeCount, _cheeseCount);
                if (_cheeseCount == CheeseWinCount)
                {
                    WinServerRpc(WinType.Cheese);
                }
                break;
            case Collectible.CollectibleType.Hammer:
                if (controller.hammer == null)
                {
                    CreatePlayerHammerServerRpc();
                }
                RequestCollectServerRpc(collectible);
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CreatePlayerHammerServerRpc()
    {
        NetworkObject obj = Instantiate(hammerPrefab, transform.position + Vector3.up, Quaternion.identity);
        obj.Spawn();
        Hammer hammer = obj.GetComponent<Hammer>();
        
        AttachHammerClientRpc(hammer.NetworkObject);
    }

    [ClientRpc]
    private void AttachHammerClientRpc(NetworkObjectReference hammerReference)
    {
        if (!IsOwner) return;
        
        if (hammerReference.TryGet(out NetworkObject obj))
        {
            Hammer hammer = obj.GetComponent<Hammer>();
        
            hammer.AttachToPlayerServerRpc(NetworkObject);
            controller.hammer = hammer;
        }
    }

    [ServerRpc]
    private void PickUpCheeseServerRpc(NetworkObjectReference collectible, int lifeCount, int cheeseCount)
    {
        PlayerManager.Instance.UpdateCardServerRpc((int)OwnerClientId, lifeCount, cheeseCount);
        RequestCollectServerRpc(collectible);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestCollectServerRpc(NetworkObjectReference collectible)
    {
        if (collectible.TryGet(out NetworkObject obj))
        {
            obj.Despawn();
        }
        else
        {
            Debug.LogWarning("Collectible could not be found on server!");
        }
    }

    public void UpdateMovementData(PlayerMovementData newMovementData)
    {
        newMovementData.direction =
            newMovementData.direction == Vector3.zero ? movementData.Value.direction : newMovementData.direction;
        movementData.Value = newMovementData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void WinServerRpc(WinType winType)
    {
        WinClientRpc(winType);
    }
    
    [ClientRpc(RequireOwnership = false)]
    private void WinClientRpc(WinType winType)
    {
        if (IsOwner)
        {
            CameraController.instance.SetTitle("You win!", true);
            string subtitle = winType == WinType.Cheese ? "Plein de coulommiers." : "Tu as gagné la bagarre.";
            CameraController.instance.SetSubtitle(subtitle, true);
            win.Value = true;
            controller.ResetOnDeath();
        }
        else
        {
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerNetwork>().OnLost((int)OwnerClientId);
        }
        
        print("sad");
        view.LaunchWinAnimation();
        LobbyManager.Instance.EnableQuitButton();
        CameraController.instance.UnlockCursor();
    }
    
    private void OnLost(int winnerId = -1)
    {
        // Dead :c
        if (!lost.Value)
        {
            CameraController.instance.SetTitle("You lost.", true);
            CameraController.instance.SetSubtitle("Cette souris a été meilleure.", true);
            
            lost.Value = true;
            
            controller.CurrentMode = PlayerController.PlayerMode.Spectate;
            
            if (winnerId == -1)
            {
                controller.ChangeSpectatePlayer(1);
            }
            else
            {
                controller.SpectateSpecificPlayer(winnerId);
            }
            
            CheckForWinner();
            
            PlayerManager.Instance.PlayerLost();
        }
    }

    [ServerRpc]
    private void SpawnCheeseOnDeathServerRpc(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 spawnPosition = transform.position +
                                    Random.Range(-2f, 2f) * Vector3.right +
                                    Random.Range(-2f, 2f) * Vector3.forward;
            
            NetworkObject obj = Instantiate(cheesePrefab, spawnPosition, Quaternion.identity);
            obj.Spawn();
        }
    }
}

public struct PlayerMovementData : INetworkSerializable
{
    public Vector3 direction;
    public float velocity;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref direction);
        serializer.SerializeValue(ref velocity);
    }
}