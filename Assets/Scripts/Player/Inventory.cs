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
        isSkinUnlocked[0] = true;

        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
        }
    }

    public void ChangeSkin()
    {
        if (skinSprites.Count == 0)
        {
            Debug.LogWarning("No skins available to change.");
            return;
        }

        do
        {
            currentSkinIndex = (currentSkinIndex + 1) % skinSprites.Count;
        } while (!isSkinUnlocked[currentSkinIndex]);

        UpdateAppearanceForSkin(currentSkinIndex);
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
}
