using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private List<Sprite> skinSprites;
    [SerializeField] private List<RuntimeAnimatorController> skinAnimators;
    [SerializeField] private List<bool> isSkinUnlocked;
    [SerializeField] private Animator playerAnimator;

    private int currentSkinIndex = 0;
    private List<string> items = new List<string>(); // New list to track items

    private void Awake()
    {
        isSkinUnlocked = new List<bool>(new bool[skinSprites.Count]);
        isSkinUnlocked[0] = true; // First skin is always unlocked

        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
        }

        LoadInventory();

        // Check and unlock skins based on owned items
        UnlockSkinsBasedOnItems();
    }

    public void ChangeSkin()
    {
        if (skinSprites.Count == 0)
        {
            Debug.LogWarning("No skins available to change.");
            return;
        }

        // Check if at least one skin is unlocked
        if (!isSkinUnlocked.Contains(true))
        {
            Debug.LogWarning("No skins unlocked to change to.");
            return;
        }

        int startingIndex = currentSkinIndex;

        while (true)
        {
            currentSkinIndex = (currentSkinIndex + 1) % skinSprites.Count;

            // Check if the current skin is unlocked
            if (isSkinUnlocked[currentSkinIndex])
            {
                UpdateAppearanceForSkin(currentSkinIndex);
                break; // Exit the loop when a valid skin is found
            }

            // If we have looped back to the starting index, stop
            if (currentSkinIndex == startingIndex)
            {
                Debug.LogWarning("No other unlocked skins available.");
                break;
            }
        }
    }


    private void UpdateAppearanceForSkin(int skinIndex)
    {
        if (skinIndex < skinSprites.Count && playerSpriteRenderer != null && playerAnimator != null)
        {
            playerSpriteRenderer.sprite = skinSprites[skinIndex];

            if (skinIndex < skinAnimators.Count && skinAnimators[skinIndex] != null)
            {
                playerAnimator.runtimeAnimatorController = skinAnimators[skinIndex];
            }
        }
        else
        {
            Debug.LogWarning("SpriteRenderer or skins not properly set.");
        }
    }

    public void UnlockSkin(int skinIndex)
    {
        if (skinIndex >= 0 && skinIndex < isSkinUnlocked.Count)
        {
            isSkinUnlocked[skinIndex] = true;
        }
        else
        {
            Debug.LogWarning("Invalid skin index.");
        }
    }
    private void UnlockSkinsBasedOnItems()
    {
        if (HasItem("Item_1"))
        {
            UnlockSkin(1); // Unlock skin 1
            Debug.Log("Skin 1 unlocked based on owning Item_1.");
        }
        else
        {
            Debug.Log("Skin 1 remains locked.");
        }

        if (HasItem("Item_2"))
        {
            UnlockSkin(2); // Unlock skin 2
            Debug.Log("Skin 2 unlocked based on owning Item_2.");
        }
        else
        {
            Debug.Log("Skin 2 remains locked.");
        }
    }

    // New method to add an item
    public void AddItem(string itemName)
    {
        if (!items.Contains(itemName))
        {
            items.Add(itemName);
            Debug.Log($"{itemName} added to inventory.");
        }
    }

    // New method to check if the inventory has a specific item
    public bool HasItem(string itemName)
    {
        return items.Contains(itemName);
    }

    public void LoadInventory()
    {
        string inventoryKey = "PlayerInventory";
        string savedInventory = PlayerPrefs.GetString(inventoryKey, "");

        if (!string.IsNullOrEmpty(savedInventory))
        {
            string[] savedItems = savedInventory.Split(',');

            foreach (var itemName in savedItems)
            {
                if (!items.Contains(itemName))
                {
                    items.Add(itemName);
                    Debug.Log($"{itemName} loaded into inventory.");
                }
            }
        }
    }

}
