using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text joinCodeText; // To display the join code
    [SerializeField] private TMP_Text timerText;    // To display the countdown timer

    private Coroutine countdownCoroutine;

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

                // Start the timer countdown
                countdownCoroutine = StartCoroutine(StartTimerCountdown(900)); // 15 minutes = 900 seconds
            }
        }
    }

    private IEnumerator StartTimerCountdown(float seconds)
    {
        float remainingTime = seconds;

        // Update the timer every second
        while (remainingTime > 0)
        {
            if (timerText != null)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTime);
                timerText.text = timeSpan.ToString(@"mm\:ss"); // Display in MM:SS format
            }
            remainingTime--;
            yield return new WaitForSecondsRealtime(1f); // Wait for 1 second
        }

        Debug.Log("15 minutes are up.");
        // Optionally, trigger host shutdown or game over logic
        if (NetworkManager.Singleton.IsHost)
        {
            HostGameManager hostGameManager = HostSingleton.Instance.GameManager as HostGameManager;
            hostGameManager?.Shutdown();
        }
    }

    public void LeaveGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.GameManager.Shutdown();
        }

        ClientSingleton.Instance.GameManager.Disconnect();
    }
}
