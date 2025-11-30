using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Random = UnityEngine.Random;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;
    
    [SerializeField] private TMP_InputField lobbyCodeTextField;
    [SerializeField] private TMP_InputField playerNameTextField;
    [SerializeField] private Transform startGameButtonTransform;

    private float _heartBeatTimer;
    private Lobby _hostLobby;
    private Lobby _joinedLobby;
    private readonly string _keyGameMode = "GameMode";
    private readonly string _keyMap = "Map";
    private readonly string _keyPlayerName = "PlayerName";
    private readonly string _keyStartGameRelayCode = "StartGameRelayCode";
    private float _lobbyUpdateTimer;
    private string _playerName;

    void Awake()
    {
        Instance = this;
    }

    async void Start()
    {
        //Sert a t'autentifier, tu t'en servira pour relier le compte steam
        // il faut installer le  Steamworks SDK
        //SignInWithSteamAsync

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        _playerName = "Mickael" + Random.Range(10, 99);
        Debug.Log("Player Name: " + _playerName);
    }

    void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPollForUpdates();
    }

    async void HandleLobbyHeartBeat()
    {
        if (_hostLobby != null)
        {
            _heartBeatTimer -= Time.deltaTime;

            if (_heartBeatTimer < 0f)
            {
                float heartbeatTimerMax = 15;
                _heartBeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
            }
        }
    }

    async void HandleLobbyPollForUpdates()
    {
        if (_joinedLobby != null)
        {
            _lobbyUpdateTimer -= Time.deltaTime;

            if (_lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                _lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
                _joinedLobby = lobby;

                if (_joinedLobby.Data[_keyStartGameRelayCode].Value != "0")
                {
                    if (!IsLobbyHost())
                    {
                        //Lobby Host already joined Relay
                        RelayManager.instance.JoinRelay(_joinedLobby.Data[_keyStartGameRelayCode].Value);
                        Debug.Log("Joining Relay");
                    }

                    _joinedLobby = null;
                }
            }
        }
    }

    [ContextMenu("Create Lobby")]
    public void CreateLobbyButton()
    {
        CreateLobby();
    }

    async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;

            CreateLobbyOptions createLobbyOptions = new()
            {
                IsPrivate = false,
                Player = GetPlayer(),

                Data = new Dictionary<string, DataObject>
                {
                    {
                        _keyGameMode,
                        new DataObject(DataObject.VisibilityOptions.Public,
                            "CaptureTheFlag" /*,DataObject.IndexOptions.S1*/)
                    },
                    { _keyMap, new DataObject(DataObject.VisibilityOptions.Public, "Dust1") },
                    { _keyStartGameRelayCode, new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            _hostLobby = lobby;
            _joinedLobby = _hostLobby;

            startGameButtonTransform.gameObject.SetActive(true);

            Debug.Log("Lobby Created      Lobby Name: " + lobby.Name + "      Max Player: " + maxPlayers +
                      "      Lobby Id: " + lobby.Id + "      LobbyCode: " + lobby.LobbyCode + "      Game Mode: " +
                      lobby.Data[_keyGameMode].Value);
            PrintPlayers(_hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [ContextMenu("List Lobbies")]
    public void ListLobbiesButton()
    {
        ListLobbies();
    }

    async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new()
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    //Filtre tout les lobby avec au moins 1 slot de libre
                    new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                    //Filtre tout les lobby sont CaptureTheFlag
                    // new QueryFilter(QueryFilter.FieldOptions.S1,"CaptureTheFlag", QueryFilter.OpOptions.EQ)
                },
                Order = new List<QueryOrder>
                {
                    new(false, QueryOrder.FieldOptions.Created)
                }
            };

            // QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            // QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);

            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log("Lobby Name: " + lobby.Name + "      Max Players: " + lobby.MaxPlayers + "      Game Mode: " +
                          lobby.Data[_keyGameMode].Value);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void JointLobbyButton()
    {
        JointLobby();
    }

    [ContextMenu("Joint Lobby")]
    async void JointLobby()
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new()
            {
                Player = GetPlayer()
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            Lobby lobby =
                await LobbyService.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id, joinLobbyByIdOptions);
            _joinedLobby = lobby;

            Debug.Log("Joined Lobby");

            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [ContextMenu("Joint Lobby By Code")]
    public void JointLobbyByCodeButton()
    {
        JointLobbyByCode(lobbyCodeTextField.text);
    }

    async void JointLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new()
            {
                Player = GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            _joinedLobby = lobby;

            Debug.Log("Joined Lobby with code: " + lobbyCode);

            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [ContextMenu("Quick Join Lobby")]
    //[Button("Quick Join Lobby")]
    public async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new()
            {
                Player = GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            _joinedLobby = lobby;

            Debug.Log("Quick Joined Lobby");

            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { _keyPlayerName, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName) }
            }
        };
    }

    [ContextMenu("Print Players")]
    public void PrintPlayers()
    {
        PrintPlayers(_joinedLobby);
    }

    void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in Lobby: " + lobby.Name + "    Lobby Data GameMode: " + lobby.Data[_keyGameMode].Value +
                  "    Map: " + lobby.Data[_keyMap].Value);

        foreach (Player player in lobby.Players)
        {
            Debug.Log("Payer Id: " + player.Id +
                      "   Player Name: " + player.Data[_keyPlayerName].Value);
        }
    }

    //[Button("Update Lobby Game Mode To Hide And Seek")]
    public void UpdateLobbyGameModeToHideAndSeek()
    {
        UpdateLobbyGameMode("HideAndSeek");
    }

    async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            _hostLobby = await LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { _keyGameMode, new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
                }
            });

            _joinedLobby = _hostLobby;

            PrintPlayers(_hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [ContextMenu("Update Player Name")]
    public void UpdatePlayerNameButton()
    {
        UpdatePlayerName(playerNameTextField.text);
    }

    async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            _playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId,
                new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {
                            _keyPlayerName,
                            new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newPlayerName)
                        }
                    }
                });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [ContextMenu("Leave Lobby")]
    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);

            Debug.Log("Leave Lobby");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [ContextMenu("Kick Player")]
    public async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, _joinedLobby.Players[1].Id);

            Debug.Log("Kick Player");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    async void MigrateLobbyHost()
    {
        try
        {
            _hostLobby = await LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = _joinedLobby.Players[1].Id
            });

            _joinedLobby = _hostLobby;

            PrintPlayers(_hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(_joinedLobby.Id);
            Debug.Log("Delete Lobby");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [ContextMenu("Start Game")]
    public async void StartGame()
    {
        if (IsLobbyHost())
        {
            try
            {
                Debug.Log("Start Game");

                string relayCode = await RelayManager.instance.CreateRelay();

                Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(_joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { _keyStartGameRelayCode, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });

                _joinedLobby = lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    bool IsLobbyHost()
    {
        if (_hostLobby != null)
        {
            return _hostLobby.HostId == AuthenticationService.Instance.PlayerId;
        }

        return false;
    }
}