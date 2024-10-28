using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private SpriteRenderer playerSpriteRenderer; // Reference to player's SpriteRenderer
    [SerializeField] private List<Sprite> skinSprites; // List of skin sprites for each skin
    [SerializeField] private List<RuntimeAnimatorController> skinAnimators; // List of animator controllers for each skin
    [SerializeField] private List<bool> isSkinUnlocked; // List to track which skins are unlocked
    [SerializeField] private Animator playerAnimator; // Reference to the player's Animator

    private int currentSkinIndex = 0;

    private void Awake()
    {
        // Initialize the unlocked skins list to false (locked)
        isSkinUnlocked = new List<bool>(new bool[skinSprites.Count]);
        isSkinUnlocked[0] = true; // Unlock the first skin (default skin)

        // Check if the Animator is assigned
        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>(); // Try to get the Animator component if not assigned
        }
    }

    public void ChangeSkin()
    {
        if (skinSprites.Count == 0)
        {
            Debug.LogWarning("No skins available to change.");
            return;
        }

        // Loop until we find an unlocked skin
        do
        {
            currentSkinIndex = (currentSkinIndex + 1) % skinSprites.Count;
        } while (!isSkinUnlocked[currentSkinIndex]);

        Debug.Log($"Changing skin to index {currentSkinIndex}");
        UpdateAppearanceForSkin(currentSkinIndex);
    }

    private void UpdateAppearanceForSkin(int skinIndex)
    {
        if (skinIndex < skinSprites.Count && playerSpriteRenderer != null && playerAnimator != null)
        {
            // Update the sprite
            Debug.Log($"Attempting to update sprite to: {skinSprites[skinIndex].name}");
            playerSpriteRenderer.sprite = skinSprites[skinIndex]; // Update sprite
            Debug.Log($"Updated sprite to: {skinSprites[skinIndex].name}");

            // Update the animator
            if (skinIndex < skinAnimators.Count && skinAnimators[skinIndex] != null)
            {
                playerAnimator.runtimeAnimatorController = skinAnimators[skinIndex]; // Update animator
                Debug.Log($"Updated animator to: {skinAnimators[skinIndex].name}");
            }
            else
            {
                Debug.LogWarning("No animator available for the current skin index.");
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
            isSkinUnlocked[skinIndex] = true; // Unlock the skin
            Debug.Log($"Skin {skinIndex} unlocked.");
        }
        else
        {
            Debug.LogWarning("Invalid skin index.");
        }
    }
}
