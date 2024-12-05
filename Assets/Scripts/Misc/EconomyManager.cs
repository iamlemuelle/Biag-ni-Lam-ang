using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EconomyManager : Singleton<EconomyManager>
{
    private TMP_Text goldText;
    private int currentGold = 0;

    private const string COIN_AMOUNT_TEXT = "Gold Amount"; // Make sure this matches the GameObject name in your scene
    private const string GOLD_PREFS_KEY = "PlayerGold";  // Key for saving gold in PlayerPrefs

    public int CurrentGold => currentGold; // Property to expose currentGold

    private void Start() {
        LoadGold();  // Load gold when the game starts
    }

    public void UpdateCurrentGold(int amount) {
        currentGold += amount; // Update current gold by adding or subtracting amount
        UpdateGoldUI();
        SaveGold();  // Save the updated gold amount
    }

    // Method to update the gold UI text
    private void UpdateGoldUI() {
        if (goldText == null) {
            GameObject goldTextObj = GameObject.Find(COIN_AMOUNT_TEXT);
            if (goldTextObj != null) {
                goldText = goldTextObj.GetComponent<TMP_Text>();
            } else {
                Debug.LogError($"GameObject with name '{COIN_AMOUNT_TEXT}' not found. Make sure it exists in the scene.");
                return; // Exit if the goldText object is not found
            }
        }

        goldText.text = currentGold.ToString("D3");
    }

    // Method to save the current gold value to PlayerPrefs
    private void SaveGold() {
        PlayerPrefs.SetInt(GOLD_PREFS_KEY, currentGold);
        PlayerPrefs.Save();  // Make sure to save the PlayerPrefs
    }

    // Method to load the saved gold value from PlayerPrefs
    private void LoadGold() {
        if (PlayerPrefs.HasKey(GOLD_PREFS_KEY)) {
            currentGold = PlayerPrefs.GetInt(GOLD_PREFS_KEY);
        } else {
            Debug.LogWarning("No saved gold found. Initializing PlayerGold to 0.");
            currentGold = 0; // Initialize currentGold to 0 if no saved data exists
            PlayerPrefs.SetInt(GOLD_PREFS_KEY, currentGold); // Set the default value in PlayerPrefs
            PlayerPrefs.Save(); // Save the PlayerPrefs to ensure the value is stored
        }
        UpdateGoldUI();  // Update the UI after loading or initializing the gold
    }
}
