using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkServer : IDisposable
{
    private NetworkManager networkManager;
    private NetworkObject playerPrefab;

    public Action<UserData> OnUserJoined;
    public Action<UserData> OnUserLeft;

    public Action<string> OnClientLeft;

    private Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>();
    private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();

    public NetworkServer(NetworkManager networkManager, NetworkObject playerPrefab)
    {
        this.networkManager = networkManager;
        this.playerPrefab = playerPrefab;

        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.OnServerStarted += OnNetworkReady;
    }

    public bool OpenConnection(string ip, int port)
    {
        UnityTransport transport = networkManager.gameObject.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, (ushort)port);
        return networkManager.StartServer();
    }

   private void ApprovalCheck(
    NetworkManager.ConnectionApprovalRequest request,
    NetworkManager.ConnectionApprovalResponse response)
{
    // Check if the payload is null or empty
    if (request.Payload == null || request.Payload.Length == 0)
    {
        Debug.LogError("Received invalid or empty payload.");
        response.Approved = false;
        return;
    }

    // Deserialize the UserData from the payload
    string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
    UserData userData;
    try
    {
        userData = JsonUtility.FromJson<UserData>(payload);
        if (userData == null)
        {
            throw new Exception("Failed to deserialize UserData.");
        }
    }
    catch (Exception e)
    {
        Debug.LogError($"Error deserializing UserData: {e.Message}");
        response.Approved = false;
        return;
    }

    // Check if a player with this authId is already in the game
    if (authIdToUserData.ContainsKey(userData.userAuthId))
    {
        Debug.LogWarning($"User with Auth ID {userData.userAuthId} is already in the game.");
        response.Approved = false;
        return;
    }

    // Map Client ID to Auth ID
    clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
    authIdToUserData[userData.userAuthId] = userData;
    OnUserJoined?.Invoke(userData);

    // Log the mapping
    Debug.Log($"Mapped Client ID {request.ClientNetworkId} to Auth ID {userData.userAuthId}");

    // Set to not create a default player object
    response.Approved = true;
    response.CreatePlayerObject = true;

    // Spawn the player object for this client
    _ = SpawnPlayerDelayed(request.ClientNetworkId);
}

private async Task SpawnPlayerDelayed(ulong clientId)
{
    Debug.Log($"Delaying player spawn for Client ID: {clientId}...");

    await Task.Delay(2000);

    if (!clientIdToAuth.TryGetValue(clientId, out string authId))
    {
        Debug.LogWarning($"No authId found for Client ID {clientId}, cannot spawn player.");
        return;
    }

    if (playerPrefab == null)
    {
        Debug.LogError("PlayerPrefab is not assigned.");
        return;
    }

    Vector3 spawnPosition = SpawnPoint.GetRandomSpawnPos();
    Debug.Log($"Spawning player at {spawnPosition} for Client ID {clientId}");

    NetworkObject playerInstance = GameObject.Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    
    playerInstance.SpawnAsPlayerObject(clientId);
    Debug.Log($"Player spawned for Client ID {clientId} with Auth ID {authId}");

    // Call any initialization method if needed
    playerInstance.GetComponent<TankPlayer>().Initialize(authId);
}




    private void OnNetworkReady()
    {
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;

        // Manually spawn the host player
        ulong hostClientId = networkManager.LocalClientId;
        _ = SpawnPlayerDelayed(hostClientId); // Call the spawn method for the host
    }

    private void OnClientDisconnect(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected");

        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            Debug.Log($"Client ID: {clientId} is mapped to Auth ID: {authId}. Removing from dictionaries...");

            clientIdToAuth.Remove(clientId);

            if (authIdToUserData.TryGetValue(authId, out UserData userData))
            {
                OnUserLeft?.Invoke(userData);
                authIdToUserData.Remove(authId);
            }

            OnClientLeft?.Invoke(authId);
        }
        else
        {
            Debug.LogWarning($"No mapping found for Client ID: {clientId} during disconnection.");
        }
    }


    public UserData GetUserDataByClientId(ulong clientId)
    {
        Debug.Log($"Fetching UserData for Client ID: {clientId}");

        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            Debug.Log($"Found Auth ID: {authId} for Client ID: {clientId}");

            if (authIdToUserData.TryGetValue(authId, out UserData data))
            {
                return data;
            }
            else
            {
                Debug.LogWarning($"No UserData found for Auth ID: {authId}");
            }
        }
        else
        {
            Debug.LogWarning($"No Auth ID mapping found for Client ID: {clientId}");
        }

        return null;
    }


    public void Dispose()
    {
        if (networkManager == null) { return; }

        networkManager.ConnectionApprovalCallback -= ApprovalCheck;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        networkManager.OnServerStarted -= OnNetworkReady;

        if (networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }
}
