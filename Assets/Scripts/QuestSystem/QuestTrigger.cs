using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    private QuestManager theQM;
    private PlayerControls playerControls; // Reference to PlayerControls
    public int questNumber;
    public bool startQuest;
    public bool endQuest;

    void Awake()
    {
        playerControls = new PlayerControls(); // Initialize PlayerControls
    }

    void Start()
    {
        theQM = FindFirstObjectByType<QuestManager>();
        playerControls.Enable(); // Enable the input actions
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "Lam-Ang")
        {
            Debug.Log("Player is within dialogue range.");
            playerControls.Combat.Dash.performed += OnDashPerformed;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.name == "Lam-Ang")
        {
            Debug.Log("Player exited dialogue range.");
            playerControls.Combat.Dash.performed -= OnDashPerformed;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe when this object is disabled
        playerControls.Combat.Dash.performed -= OnDashPerformed;
        playerControls.Disable();
    }

    private void OnDashPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        Debug.Log("Dash button pressed inside dialogue range.");
        showDiag();
    }

    public void showDiag()
    {
        Debug.Log("Dash button pressed.");
        // Ensure questNumber is within bounds
        if (questNumber < theQM.questCompleted.Length && questNumber < theQM.quests.Length)
        {
            // Check if the quest object exists
            if (theQM.quests[questNumber] != null)
            {
                // Log the state of the quest object
                Debug.Log("Quest " + questNumber + " is present with active state: " + theQM.quests[questNumber].gameObject.activeInHierarchy);

                // Check if the quest is active
                if (!theQM.questCompleted[questNumber])
                {
                    if (startQuest && !theQM.quests[questNumber].gameObject.activeSelf)
                    {
                        Debug.Log("Reactivating Quest " + questNumber);
                        theQM.quests[questNumber].gameObject.SetActive(true);
                        theQM.quests[questNumber].StartQuest();
                    }

                    if (endQuest && theQM.quests[questNumber].gameObject.activeSelf)
                    {
                        theQM.quests[questNumber].EndQuest();
                    }

                    // Trigger dialogue display for the quest
                    theQM.quests[questNumber].StartQuest(); // This will handle showing the correct dialogue
                }
                else
                {
                    // If the quest is completed, you may want to show a different dialogue
                    Debug.Log("Quest " + questNumber + " is already completed.");
                }
            }
            else
            {
                Debug.LogWarning("Quest " + questNumber + " is missing or has been destroyed.");
            }
        }
        else
        {
            Debug.LogError("Quest number is out of bounds: " + questNumber);
        }
    }
}