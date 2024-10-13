using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueHolder : MonoBehaviour
{
    private DialogueManager dMAn;
    private PlayerControls playerControls; // Reference to PlayerControls

    // Arrays to store dialogues for different languages
    [Header("Tagalog Dialogue")]
    [SerializeField] private string[] dialogueLinesTagalog;

    [Header("Ilocano Dialogue")]
    [SerializeField] private string[] dialogueLinesIlocano;

    [Header("Character Images")]
    public Image playerImage;    // Right-side player portrait
    public Image npcImage;       // Left-side NPC portrait

    public Sprite[] playerImagesTagalog; // Array for player portraits in Tagalog
    public Sprite[] npcImagesTagalog;    // Array for NPC portraits in Tagalog

    public Sprite[] playerImagesIlocano; // Array for player portraits in Ilocano
    public Sprite[] npcImagesIlocano;    // Array for NPC portraits in Ilocano

    void Awake()
    {
        playerControls = new PlayerControls(); // Initialize PlayerControls
    }

    void Start()
    {
        // Find the DialogueManager by name
        GameObject dialogueManagerObject = GameObject.Find("Dialogue Manager");
        if (dialogueManagerObject != null)
        {
            dMAn = dialogueManagerObject.GetComponent<DialogueManager>();
        }
        else
        {
            Debug.LogError("Dialogue Manager not found in the scene!");
        }

        // Find player and NPC images if they are not assigned
        if (playerImage == null)
        {
            playerImage = GameObject.Find("Dialogue Box/Character").GetComponent<Image>(); // Adjust the path as necessary
        }

        if (npcImage == null)
        {
            npcImage = GameObject.Find("Dialogue Box/NPC").GetComponent<Image>(); // Adjust the path as necessary
        }

        playerControls.Enable(); // Enable the input actions
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.name == "Lam-Ang")
        {
            // Check for South button press
            if (playerControls.Combat.Dash.triggered) // Make sure this matches your South button mapping
            {
                if (!dMAn.dialogActive)
                {
                    // Get the current language from the LanguageManager
                    string currentLanguage = LanguageManager.Instance.currentLanguage;

                    // Assign the correct dialogue lines based on the current language
                    if (currentLanguage == "tag")
                    {
                        dMAn.dialogLines = dialogueLinesTagalog;
                    }
                    else if (currentLanguage == "il")
                    {
                        dMAn.dialogLines = dialogueLinesIlocano;
                    }
                    else
                    {
                        Debug.LogError("Unsupported language: " + currentLanguage);
                    }

                    dMAn.currentLine = 0;  // Start from the first line
                    UpdateCharacterImage(); // Update the images based on the first line
                    dMAn.ShowDialogue();    // Display the dialogue box
                }
            }
        }
    }

    public void TriggerDialogue()
    {
        if (dMAn != null)
        {
            // Get the current language from the LanguageManager
            string currentLanguage = LanguageManager.Instance.currentLanguage;

            // Assign the correct dialogue lines based on the current language
            if (currentLanguage == "tag")
            {
                dMAn.dialogLines = dialogueLinesTagalog;
            }
            else if (currentLanguage == "il")
            {
                dMAn.dialogLines = dialogueLinesIlocano;
            }
            else
            {
                Debug.LogError("Unsupported language: " + currentLanguage);
            }

            dMAn.currentLine = 0;  // Start from the first line
            dMAn.ShowDialogue();    // Display the dialogue box
            UpdateCharacterImage(); // Update the images based on the first line
        }
    }

    public void UpdateCharacterImage()
    {
        // Check if the Image references are null and attempt to find them if they are
        if (playerImage == null)
        {
            playerImage = GameObject.Find("Character")?.GetComponent<Image>();
        }
        
        if (npcImage == null)
        {
            npcImage = GameObject.Find("NPC")?.GetComponent<Image>();
        }

        if (dMAn.dialogLines.Length > dMAn.currentLine)
        {
            string currentLine = dMAn.dialogLines[dMAn.currentLine];
            string currentLanguage = LanguageManager.Instance.currentLanguage;

            // Determine if the player or NPC is speaking based on line markers
            bool isPlayerSpeaking = currentLine.StartsWith("[Player]");

            // Debug log to check which line is being processed
            Debug.Log($"Updating character image for line: {currentLine} | Player Speaking: {isPlayerSpeaking}");

            // Select the appropriate sprites based on the current language
            Sprite[] playerImages = (currentLanguage == "tag") ? playerImagesTagalog : playerImagesIlocano;
            Sprite[] npcImages = (currentLanguage == "tag") ? npcImagesTagalog : npcImagesIlocano;

            // Update images based on who is speaking
            if (isPlayerSpeaking)
            {
                if (playerImages.Length > dMAn.currentLine)
                {
                    playerImage.sprite = playerImages[dMAn.currentLine]; // Update to the correct player sprite
                    playerImage.gameObject.SetActive(true);
                    npcImage.gameObject.SetActive(false); // Hide NPC portrait when player speaks
                }
            }
            else
            {
                if (npcImages.Length > dMAn.currentLine)
                {
                    npcImage.sprite = npcImages[dMAn.currentLine]; // Update to the correct NPC sprite
                    npcImage.gameObject.SetActive(true);
                    playerImage.gameObject.SetActive(false); // Hide player portrait when NPC speaks
                }
            }
        }
    }




    // Set the appropriate character images for the dialogue line
    private void SetCharacterImages(bool isPlayerSpeaking, Sprite[] playerImages, Sprite[] npcImages)
    {
        // Display player image when the player is speaking
        if (isPlayerSpeaking)
        {
            playerImage.gameObject.SetActive(true);
            npcImage.gameObject.SetActive(false); // Hide NPC portrait when player speaks
        }
        else // NPC is speaking
        {
            npcImage.gameObject.SetActive(true);
            playerImage.gameObject.SetActive(false); // Hide player portrait when NPC speaks
        }
    }

    // Check if the dialogue has ended
    public bool IsDialogueEnded()
    {
        return !dMAn.dialogActive; // Returns true if the dialogue is not active
    }

    private void OnDisable()
    {
        playerControls.Disable(); // Disable input actions when not in use
    }
}
