using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI; // Or use TMPro if you are using TextMesh Pro
using TMPro;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text joinCodeText; // Or TMP_Text if using TextMesh Pro

    private void Start()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            // Get the HostGameManager instance and display the join code
            HostGameManager hostGameManager = HostSingleton.Instance.GameManager as HostGameManager;

            if (hostGameManager != null)
            {
                // Update the join code in the HUD
                string joinCode = hostGameManager.GetJoinCode();
                joinCodeText.text = $"Join Code: {joinCode}";
            }
        }
    }

    // public void LeaveGame()
    // {
    //     if (NetworkManager.Singleton.IsHost)
    //     {
    //         HostSingleton.Instance.GameManager.Shutdown();
    //     }

    //     ClientSingleton.Instance.GameManager.Disconnect();
    // }
}
