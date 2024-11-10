using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public string itemName;

    // Save this item's collection status
    public void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetInt($"Collected_{itemName}", 1);  // 1 indicates collected
        PlayerPrefs.Save();
    }

    // Check if this item was collected in a previous session
    public bool IsCollected()
    {
        return PlayerPrefs.GetInt($"Collected_{itemName}", 0) == 1;  // Default to 0 (not collected)
    }
}