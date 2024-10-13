using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EconomyManager : Singleton<EconomyManager>
{
    private TMP_Text goldText;
    private int currentGold = 0;

    const string COIN_AMOUNT_TEXT = "Gold Amount";
    const string GOLD_PREFS_KEY = "PlayerGold";  // Key for saving gold in PlayerPrefs

    private void Start() {
        LoadGold();  // Load gold when the game starts
    }

    public void UpdateCurrentGold() {
        currentGold += 1;
        UpdateGoldUI();
        SaveGold();  // Save the updated gold amount
    }

    // Method to update the gold UI text
    private void UpdateGoldUI() {
        if (goldText == null) {
            goldText = GameObject.Find(COIN_AMOUNT_TEXT).GetComponent<TMP_Text>();
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
            UpdateGoldUI();  // Update the UI after loading the gold
        }
    }
}
