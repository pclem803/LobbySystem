using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using Steamworks;
using FishNet.Managing;
using UnityEngine.UI;
using FishNet.Managing.Transporting;
using TMPro;

public class SteamLobby : MonoBehaviour
{
    // Callbacks -> Funtions that are called on steam actions
    protected Callback<LobbyCreated_t> LobbryCreated;
    protected Callback<GameLobbyJoinRequested_t> GameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> LobbyEntered;


    // Variables
    public ulong CurrentLobbyID;
    private const string HostAddressKey = "HostAddress";

    // Game Objects
    public GameObject hostButton;
    public TMP_Text LobbyNameText;

    private void Start()
    {
        if (!SteamManager.Initialized) { return; }

        LobbryCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        GameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
    }

    // Callback Functions
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        // if lobby has been created successfully
        if (callback.m_eResult != EResult.k_EResultOK) return;

        Debug.Log("Lobby Created Succesfully");

        // Start Host
        InstanceFinder.ServerManager.StartConnection();
        InstanceFinder.ClientManager.StartConnection();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString() + "'s Lobby");
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Request To Join Lobby");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        // Everyone
        hostButton.SetActive(false);
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        LobbyNameText.gameObject.SetActive(true);
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");

        // Client
        if (InstanceFinder.IsServer) { return; }

        string address = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

        InstanceFinder.ClientManager.StartConnection(address);
    }
}
