using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ShopManagerScript : MonoBehaviour
{
    public int[,] shopItems = new int[5, 7];
    private EconomyManager economyManager; // Reference to the EconomyManager
    public TMP_Text CoinsTXT; 

    void Start()
    {
        economyManager = EconomyManager.Instance; // Get the instance of the EconomyManager
        UpdateCoinsText();
        InitializeShopItems();
        LoadPurchasedItems(); // Load purchased item quantities
    }

    private void InitializeShopItems()
    {
        // IDs
        shopItems[1, 1] = 1;
        shopItems[1, 2] = 2;
        shopItems[1, 3] = 3;
        shopItems[1, 4] = 4;
        shopItems[1, 5] = 5;
        shopItems[1, 6] = 6;

        // Price
        shopItems[2, 1] = 1000;
        shopItems[2, 2] = 1000;
        shopItems[2, 3] = 1500;
        shopItems[2, 4] = 3500;
        shopItems[2, 5] = 4440;
        shopItems[2, 6] = 5000;

        // Quantity (default is 0, will be overwritten if loaded)
        shopItems[3, 1] = 0;
        shopItems[3, 2] = 0;
        shopItems[3, 3] = 0;
        shopItems[3, 4] = 0;
        shopItems[3, 5] = 0;
        shopItems[3, 6] = 0;
    }

    public void Buy()
    {
        GameObject ButtonRef = GameObject.FindGameObjectWithTag("Event").GetComponent<EventSystem>().currentSelectedGameObject;

        int itemID = ButtonRef.GetComponent<ItemInfo>().ItemID;
        float currentCoins = economyManager.CurrentGold; // Get current gold from EconomyManager

        if (currentCoins >= shopItems[2, itemID])
        {
            economyManager.UpdateCurrentGold(-shopItems[2, itemID]); // Deduct coins from economy manager
            shopItems[3, itemID]++;
            SavePurchasedItem(itemID); // Save the purchased item's quantity
            UpdateCoinsText(); // Update the coins text after purchase
            ButtonRef.GetComponent<ItemInfo>().QuantityTxt.text = shopItems[3, itemID].ToString();
        }
    }

    private void UpdateCoinsText() // Update the coins text
    {
        CoinsTXT.text = "Coins: " + economyManager.CurrentGold.ToString();
    }

    private void SavePurchasedItem(int itemID)
    {
        // Save the purchased quantity for the specific itemID
        PlayerPrefs.SetInt($"ShopItem_{itemID}_Quantity", shopItems[3, itemID]);

        // Add item to the player's inventory in PlayerPrefs
        string inventoryKey = "PlayerInventory";
        List<string> inventory = new List<string>(PlayerPrefs.GetString(inventoryKey, "").Split(','));
        string itemName = $"Item_{itemID}";

        if (!inventory.Contains(itemName)) // Avoid duplicates
        {
            inventory.Add(itemName);
            PlayerPrefs.SetString(inventoryKey, string.Join(",", inventory));
        }

        PlayerPrefs.Save();
    }


    private void LoadPurchasedItems()
    {
        // Load quantities for each item
        for (int itemID = 1; itemID < shopItems.GetLength(1); itemID++)
        {
            if (PlayerPrefs.HasKey($"ShopItem_{itemID}_Quantity"))
            {
                shopItems[3, itemID] = PlayerPrefs.GetInt($"ShopItem_{itemID}_Quantity");
            }
        }

        // Update the UI for all items
        foreach (var button in FindObjectsOfType<ItemInfo>())
        {
            int itemID = button.ItemID;
            if (itemID > 0 && itemID < shopItems.GetLength(1))
            {
                button.QuantityTxt.text = shopItems[3, itemID].ToString();
            }
        }
    }
}
