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
        theQM = FindObjectOfType<QuestManager>();
        playerControls.Enable(); // Enable the input actions
    }

    private void OnTriggerStay2D(Collider2D other) 
    {
        if (other.gameObject.name == "Lam-Ang")
        {
            // Check for South button press
            if (playerControls.Combat.Dash.triggered) // Make sure this matches your South button mapping
            {
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
    }

    private void OnDisable()
    {
        playerControls.Disable(); // Disable the input actions when not in use
    }
}
