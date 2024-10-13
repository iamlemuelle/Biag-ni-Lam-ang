using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : IDisposable
{
    private JoinAllocation allocation;

    private NetworkClient networkClient;
    // private MatchplayMatchmaker matchmaker;
    
    public UserData UserData;

    private const string MenuSceneName = "Menu";

    public async Task<bool> InitAsync()
{
    try
    {
        Debug.Log("Initializing Unity Services...");
        await UnityServices.InitializeAsync();  // Make sure this line is awaited properly

        // Subscribe to authentication sign-in event
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"Player signed in with ID: {AuthenticationService.Instance.PlayerId}");
        };

        Debug.Log("Unity Services initialized successfully.");
        return true;
    }
    catch (Exception ex)
    {
        Debug.LogError($"Failed to initialize Unity Services: {ex.Message}");
        return false;
    }
}

    public void GoToMenu()
    {
        SceneManager.LoadScene(MenuSceneName);
    }

    public void StartClient(string ip, int port)
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, (ushort)port);
        ConnectClient();
    }

    public async Task StartClientAsync(string joinCode)
    {
        try
        {
            allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to join allocation: {e.Message}");
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        // Ensure UserData is initialized
        if (UserData == null)
        {
            UserData = new UserData
            {
                userName = "PlayerName", // or any default name
                userAuthId = AuthenticationService.Instance.PlayerId
            };

            Debug.Log($"Initializing UserData with PlayerId: {AuthenticationService.Instance.PlayerId}");
        }

        ConnectClient();
    }



    public void ConnectClient()
    {
        if (UserData == null)
        {
            Debug.LogError("UserData is null before connecting.");
            return;
        }

        string payload = JsonUtility.ToJson(UserData);
        Debug.Log($"Sending UserData: {payload}");
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        // Adjust client ID for logic purposes
        ulong adjustedClientId = NetworkManager.Singleton.LocalClientId + 1;

        Debug.Log($"Starting client connection with adjusted Client ID: {adjustedClientId}");
        NetworkManager.Singleton.StartClient();
    }




    // public async void MatchmakeAsync(Action<MatchmakerPollingResult> onMatchmakeResponse)
    // {
    //     if (matchmaker.IsMatchmaking)
    //     {
    //         return;
    //     } 

    //     // UserData.userGamePreferences.gameQueue = isTeamQueue ? GameQueue.Team : GameQueue.Solo;
    //     MatchmakerPollingResult matchResult = await GetMatchAsync();
    //     onMatchmakeResponse?.Invoke(matchResult);
    // }

    // private async Task<MatchmakerPollingResult> GetMatchAsync()
    // {
    //     MatchmakingResult matchmakingResult = await matchmaker.Matchmake(UserData);

    //     if (matchmakingResult.result == MatchmakerPollingResult.Success)
    //     {
    //         StartClient(matchmakingResult.ip, matchmakingResult.port);
    //     }

    //     return matchmakingResult.result;
    // }

    // public async Task CancelMatchmaking()
    // {
    //     await matchmaker.CancelMatchmaking();
    // }

    public void Disconnect()
    {
        networkClient.Disconnect();
    }

    public void Dispose()
    {
        networkClient?.Dispose();
    }
}