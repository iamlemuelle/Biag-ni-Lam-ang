using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class AreaExit : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private string sceneTransitionName;
    [SerializeField] private DialogueHolder dialogueHolder; // Reference to DialogueHolder for triggering dialogue
    [SerializeField] private int requiredLevel = 5;

    private float waitToLoadTime = 1f;
    private bool dialogueEnded = false;
    public TMP_Text levelFeedbackText;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();

            if (playerController.currentLevel >= requiredLevel)
            {
                SceneManagement.Instance.SetTransitionName(sceneTransitionName); // Keep the existing scene transition logic

                UIFade.Instance.FadeToBlack(false);

                if (!dialogueEnded)
                {
                    StartCoroutine(WaitForDialogueToEnd());
                }
            }
            else
            {
                StartCoroutine(ShowLevelFeedback("Player level is too low to enter this area."));
            }
        }
    }

    private IEnumerator ShowLevelFeedback(string message)
    {
        levelFeedbackText.text = message;
        levelFeedbackText.gameObject.SetActive(true); // Show the feedback text

        yield return new WaitForSeconds(4); // Wait for 2 seconds

        levelFeedbackText.gameObject.SetActive(false); // Hide the feedback text
    }
    private IEnumerator WaitForDialogueToEnd() 
    {
        // Trigger the specific dialogue in DialogueHolder
        dialogueHolder.TriggerDialogue(); // Ensure this method exists in DialogueHolder

        // Wait until the dialogue ends
        while (!dialogueHolder.IsDialogueEnded()) 
        {
            yield return null; // Wait for the dialogue to finish
        }

        dialogueEnded = true; // Mark dialogue as ended

        // Activate the fade text after the dialogue ends
        UIFade.Instance.SetTextActive(true);

        // Wait a bit before loading the new scene
        yield return new WaitForSeconds(waitToLoadTime);

        UIFade.Instance.DestroyUI_Canvas();

        // Load the next scene
        UIFade.Instance.LoadingScreen();
        yield return SceneManager.LoadSceneAsync(sceneToLoad);
    }

    private void DestroyUICanvas()
    {
        // Find the UI_Canvas object by name
        GameObject uiCanvas = GameObject.Find("UI_Canvas");

        // Check if the object exists and destroy it
        if (uiCanvas != null)
        {
            Debug.Log("Destroying UI_Canvas");
            Destroy(uiCanvas);
        }
        else
        {
            Debug.Log("UI_Canvas not found");
        }
    }

}
