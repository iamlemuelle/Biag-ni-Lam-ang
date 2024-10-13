using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class ServerGameManager : IDisposable
{
    private string serverIP;
    private int serverPort;
    private int queryPort;
    public NetworkServer NetworkServer { get; private set; }

    public ServerGameManager(string serverIP, int serverPort, int queryPort, NetworkManager manager, NetworkObject playerPrefab)
    {
        this.serverIP = serverIP;
        this.serverPort = serverPort;
        this.queryPort = queryPort;
        NetworkServer = new NetworkServer(manager, playerPrefab);
    }

    public async Task StartGameServerAsync()
    {
        try
        {
            // Add any initialization logic here if needed

            // Attempt to open connection to the server
            if (!NetworkServer.OpenConnection(serverIP, serverPort))
            {
                Debug.LogWarning("NetworkServer did not start as expected.");
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    public void Dispose()
    {
        NetworkServer?.Dispose();
    }
}
