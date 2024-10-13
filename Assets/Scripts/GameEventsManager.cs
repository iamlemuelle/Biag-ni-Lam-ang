using System;
using UnityEngine;

public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager instance { get; private set; }

    public PlayerController playerController;
    public EconomyManager economyManager;
    public PlayerHealth playerHealth;
    public Stamina stamina;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Game Events Manager in the scene.");
        }
        instance = this;

        // initialize all events
        playerController = new PlayerController();
        economyManager = new EconomyManager();
        playerHealth = new PlayerHealth();
        stamina = new Stamina();
    }
}