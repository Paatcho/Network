using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;
    
    [SerializeField] private TMP_InputField lobbyCodeTextField;
    [SerializeField] private TMP_InputField playerNameTextField;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button quitGameButton;

    private float _heartBeatTimer;
    
    private Lobby _hostLobby;

    private Lobby HostLobby
    {
        get => _hostLobby;
        set
        {
            _hostLobby = value;
            UpdateButtonVisibility();
        }
    }
    
    private Lobby _joinedLobby;
    private Lobby JoinedLobby
    {
        get => _joinedLobby;
        set
        {
            _joinedLobby = value;
            UpdateButtonVisibility();
        }
    }
    
    private readonly string _keyGameMode = "GameMode";
    private readonly string _keyMap = "Map";
    private readonly string _keyPlayerName = "PlayerName";
    private readonly string _keyStartGameRelayCode = "StartGameRelayCode";
    private readonly string _keyGameState = "GameState";

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

    void UpdateButtonVisibility()
    {
        createLobbyButton.interactable = JoinedLobby != null && HostLobby != null;
        quickJoinButton.interactable = JoinedLobby != null && HostLobby != null;
        startGameButton.interactable = JoinedLobby != null && IsLobbyHost();
    }

    public void EnableQuitButton()
    {
        quitGameButton.gameObject.SetActive(true);
    }
    
    async void HandleLobbyHeartBeat()
    {
        if (HostLobby != null)
        {
            _heartBeatTimer -= Time.deltaTime;

            if (_heartBeatTimer < 0f)
            {
                float heartbeatTimerMax = 15;
                _heartBeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(HostLobby.Id);
            }
        }
    }
    
    public async void LeaveGameButton()
    {
        try
        {
            // Leave Relay
            RelayManager.instance.Disconnect();

            // Leave the lobby if you are in one
            if (_joinedLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(
                    _joinedLobby.Id, 
                    AuthenticationService.Instance.PlayerId
                );
            }

            _joinedLobby = null;
            _hostLobby = null;

            // Reload the scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );

            Debug.Log("Player left the lobby and returned to main menu");
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    async void HandleLobbyPollForUpdates()
    {
        if (JoinedLobby != null)
        {
            _lobbyUpdateTimer -= Time.deltaTime;

            if (_lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                _lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);
                JoinedLobby = lobby;

                if (JoinedLobby.Data[_keyStartGameRelayCode].Value != "0")
                {
                    if (!IsLobbyHost())
                    {
                        //Lobby Host already joined Relay
                        RelayManager.instance.JoinRelay(JoinedLobby.Data[_keyStartGameRelayCode].Value);
                        Debug.Log("Joining Relay");
                    }

                    JoinedLobby = null;
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
            int maxPlayers = 15;

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
                    { _keyStartGameRelayCode, new DataObject(DataObject.VisibilityOptions.Member, "0") },
                    { _keyGameState, new DataObject(DataObject.VisibilityOptions.Member, "Menu") }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            HostLobby = lobby;
            JoinedLobby = HostLobby;

            startGameButton.gameObject.SetActive(true);

            Debug.Log("Lobby Created      Lobby Name: " + lobby.Name + "      Max Player: " + maxPlayers +
                      "      Lobby Id: " + lobby.Id + "      LobbyCode: " + lobby.LobbyCode + "      Game Mode: " +
                      lobby.Data[_keyGameMode].Value);
            PrintPlayers(HostLobby);
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
            JoinedLobby = lobby;

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
            JoinedLobby = lobby;

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
            JoinedLobby = lobby;

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
        PrintPlayers(JoinedLobby);
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
            HostLobby = await LobbyService.Instance.UpdateLobbyAsync(HostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { _keyGameMode, new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
                }
            });

            JoinedLobby = HostLobby;

            PrintPlayers(HostLobby);
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
            await LobbyService.Instance.UpdatePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId,
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
            await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId);

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
            await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, JoinedLobby.Players[1].Id);

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
            HostLobby = await LobbyService.Instance.UpdateLobbyAsync(HostLobby.Id, new UpdateLobbyOptions
            {
                HostId = JoinedLobby.Players[1].Id
            });

            JoinedLobby = HostLobby;

            PrintPlayers(HostLobby);
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
            await LobbyService.Instance.DeleteLobbyAsync(JoinedLobby.Id);
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

                Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(JoinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { _keyStartGameRelayCode, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });

                JoinedLobby = lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    bool IsLobbyHost()
    {
        if (HostLobby != null)
        {
            return HostLobby.HostId == AuthenticationService.Instance.PlayerId;
        }

        return false;
    }
}