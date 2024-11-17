using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public GameObject dBox;         // Dialogue box UI
    public TMP_Text dText;          // TextMeshPro text element for displaying dialogue
    public bool dialogActive;       // Check if dialogue is active
    public string[] dialogLines;    // Array of dialogue lines
    public int currentLine;         // The current line being displayed

    [Header("Dialogue Lines")]
    [SerializeField] private string[] tagalogDialogues;  // Dialogue lines in Tagalog
    [SerializeField] private string[] ilocanoDialogues;  // Dialogue lines in Ilocano

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
        }
        else if (language == "il")
        {
            dialogLines = ilocanoDialogues;
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
            StopAllCoroutines();
            StartCoroutine(TypeSentence(dialogLines[currentLine]));  // Type out the next line
            // Call UpdateCharacterImage to update the character portraits
            DialogueHolder dialogueHolder = FindFirstObjectByType<DialogueHolder>(); // Assuming DialogueHolder is a component on a GameObject in the scene
            if (dialogueHolder != null)
            {
                dialogueHolder.UpdateCharacterImage(); // Ensure character images are updated
            }
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

    public void PreviousDialogue()
    {
        if (currentLine > 0)  // Ensure we don't go before the first line
        {
            currentLine--;  // Go to the previous line
            dText.text = dialogLines[currentLine];  // Display the previous line
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
