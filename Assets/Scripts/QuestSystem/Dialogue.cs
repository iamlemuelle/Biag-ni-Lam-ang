using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    public GameObject dBox;         // Dialogue box UI
    public TMP_Text dText;          // TextMeshPro text element for displaying dialogue
    public bool dialogActive;       // Check if dialogue is active
    public string[] dialogLines;    // Array of dialogue lines
    public int currentLine;         // The current line being displayed

    [Header("Dialogue Lines")]
    [SerializeField] private string[] tagalogDialogues;  // Dialogue lines in Tagalog
    [SerializeField] private string[] ilocanoDialogues;  // Dialogue lines in Ilocano

    [Header("Character Images")]
    public Image characterImage;    // UI Image component for displaying character portrait
    public Sprite[] characterImagesTagalog; // Array of character images for Tagalog
    public Sprite[] characterImagesIlocano; // Array of character images for Ilocano

    private Sprite[] currentCharacterImages; // Array of character images based on the current language

    void Start()
    {
        dialogActive = false;
        currentLine = 0;

        // Set the initial language, for example, "tag" for Tagalog
        SetLanguage("tag");
    }

    void Update()
    {
        // Progress through dialogue with spacebar
        if (dialogActive && Input.GetKeyDown(KeyCode.Space))
        {
            ContinueDialogue();  // Call the ContinueDialogue method on spacebar press
        }
    }

    // Method to set the language and show the corresponding dialogue
    public void SetLanguage(string language)
    {
        if (language == "tag")
        {
            dialogLines = tagalogDialogues;
            currentCharacterImages = characterImagesTagalog;  // Set the correct character images for Tagalog
        }
        else if (language == "il")
        {
            dialogLines = ilocanoDialogues;
            currentCharacterImages = characterImagesIlocano;  // Set the correct character images for Ilocano
        }
        else
        {
            Debug.LogError("Unsupported language: " + language);
            return;
        }

        ShowDialogue(); // Show the dialogue after setting the language
    }

    // This method starts the dialogue with the array of dialogue lines
    public void ShowDialogue()
    {
        if (dialogLines.Length > 0)
        {
            dialogActive = true;
            dBox.SetActive(true);
            currentLine = 0;    // Start from the first line
            StopAllCoroutines();
            StartCoroutine(TypeSentence(dialogLines[currentLine])); // Type out the first line
            UpdateCharacterImage();  // Set the initial character image
        }
        else
        {
            Debug.LogError("No dialogue lines available to show.");
        }
    }

    // Continue through the dialogue
    public void ContinueDialogue()
    {
        currentLine++;  // Move to the next line

        // If we've reached the end of the dialogue array, close the dialogue box
        if (currentLine >= dialogLines.Length)
        {
            EndDialogue();
        }
        else
        {
            StopAllCoroutines(); // Stop the typing coroutine
            StartCoroutine(TypeSentence(dialogLines[currentLine]));  // Type out the next line
            UpdateCharacterImage();  // Update the character image for the next line
        }
    }

    // Coroutine to type out each sentence letter by letter
    IEnumerator TypeSentence(string sentence)
    {
        dText.text = "";  // Clear the text before typing starts
        foreach (char letter in sentence.ToCharArray())
        {
            dText.text += letter;
            yield return new WaitForSeconds(0.01f);  // Adjust the speed of typing here
        }
    }

    // Update the character image based on the current line of dialogue
    private void UpdateCharacterImage()
    {
        if (characterImage != null && currentCharacterImages != null)
        {
            if (currentLine < currentCharacterImages.Length && currentCharacterImages[currentLine] != null)
            {
                characterImage.sprite = currentCharacterImages[currentLine];  // Update the image based on the current line
                characterImage.gameObject.SetActive(true);  // Ensure the image is visible
            }
            else
            {
                characterImage.gameObject.SetActive(false); // Hide the image if no more images are available
            }
        }
    }

    // Go back to the previous dialogue line
    public void PreviousDialogue()
    {
        if (currentLine > 0)  // Ensure we don't go before the first line
        {
            currentLine--;  // Go to the previous line
            StopAllCoroutines(); // Stop the typing coroutine
            StartCoroutine(TypeSentence(dialogLines[currentLine]));  // Type out the previous line
            UpdateCharacterImage();  // Update the character image for the previous line
        }
    }

    // Close the dialogue box and reset variables
    public void EndDialogue()
    {
        dBox.SetActive(false);
        dialogActive = false;
        currentLine = 0; // Reset current line for the next time dialogue is triggered
    }
}
