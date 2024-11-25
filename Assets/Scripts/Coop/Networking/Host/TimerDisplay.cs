using System;
using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    private GameTimer gameTimer;

    private void Start()
{
    Debug.Log("TimerDisplay: Starting setup...");
    gameTimer = GameObject.Find("GameTimerManager")?.GetComponent<GameTimer>();

    if (gameTimer != null)
    {
        Debug.Log("TimerDisplay: GameTimer found. Subscribing to OnTimeUpdated...");
        gameTimer.OnTimeUpdated += UpdateTimerUI;
    }
    else
    {
        Debug.LogError("TimerDisplay: GameTimer not found.");
    }
}

private void UpdateTimerUI(float remainingTime)
{
    TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTime);
    timerText.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    Debug.Log($"TimerDisplay: Timer updated to {timerText.text}");
}


    private void OnDestroy()
    {
        if (gameTimer != null)
        {
            gameTimer.OnTimeUpdated -= UpdateTimerUI;
        }
    }
}
