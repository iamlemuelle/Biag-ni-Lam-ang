using System;
using Unity.Netcode;
using UnityEngine;

public class GameTimer : NetworkBehaviour
{
    [SerializeField] private float totalGameTime = 15 * 60f; // 15 minutes in seconds
    private NetworkVariable<float> remainingTime = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone);

    public event Action<float> OnTimeUpdated; // Event for updating the UI

    private bool timerRunning = false;

    private void Start()
    {
        if (IsServer)
        {
            remainingTime.Value = totalGameTime;
        }
    }

    private void Update()
    {
        if (!timerRunning || !IsServer) return;

        // Decrement remaining time
        remainingTime.Value -= Time.deltaTime;

        // Notify clients about the updated time
        OnTimeUpdated?.Invoke(remainingTime.Value);

        if (remainingTime.Value <= 0)
        {
            timerRunning = false;
            remainingTime.Value = 0;
            EndGame();
        }
    }

    public void StartTimer()
    {
        if (!IsServer) return;

        timerRunning = true;
        remainingTime.Value = totalGameTime;
        Debug.Log($"GameTimer: Timer started with {totalGameTime} seconds.");
    }

    private void EndGame()
    {
        Debug.Log("Time is up! Ending the game...");
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Menu", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    public override void OnNetworkSpawn()
    {
        // Subscribe to changes in the remaining time
        if (!IsServer)
        {
            remainingTime.OnValueChanged += HandleTimeChanged;
        }
    }

    private void HandleTimeChanged(float oldValue, float newValue)
    {
        Debug.Log($"GameTimer: Time changed from {oldValue} to {newValue}");
        OnTimeUpdated?.Invoke(newValue);
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        if (!IsServer)
        {
            remainingTime.OnValueChanged -= HandleTimeChanged;
        }
    }
}
