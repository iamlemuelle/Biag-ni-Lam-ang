using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaExit : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private string sceneTransitionName;
    [SerializeField] private DialogueHolder dialogueHolder; // Reference to DialogueHolder for triggering dialogue

    private float waitToLoadTime = 1f;
    private bool dialogueEnded = false;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.gameObject.GetComponent<PlayerController>()) 
        {
            SceneManagement.Instance.SetTransitionName(sceneTransitionName); // Keep the existing scene transition logic
            
            // Start fading to black without activating text yet
            UIFade.Instance.FadeToBlack(false);

            // Start the dialogue before loading the new scene
            if (!dialogueEnded) 
            {
                StartCoroutine(WaitForDialogueToEnd());
            }
        }
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

        // Load the next scene
        SceneManager.LoadScene(sceneToLoad);
    }
}
