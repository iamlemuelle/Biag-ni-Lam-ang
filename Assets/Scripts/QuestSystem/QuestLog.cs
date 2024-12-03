using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestLog : MonoBehaviour
{
    [SerializeField] private Button[] questButtons; // Array of buttons corresponding to quests

    public void InitializeQuestLog(QuestManager questManager)
    {
        // Initialize buttons with quest descriptions
        for (int i = 0; i < questButtons.Length && i < questManager.quests.Length; i++)
        {
            int questNumber = i; // Local copy for button closure
            questButtons[i].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = questManager.quests[i].GetDescription();
            questButtons[i].gameObject.SetActive(true); // Ensure button is active
            questButtons[i].onClick.RemoveAllListeners(); // Clear any existing listeners
            questButtons[i].onClick.AddListener(() => Debug.Log($"Quest {questNumber} clicked!"));
        }
    }

    public void UpdateQuestLog(QuestManager questManager)
    {
        // Update button states based on quest completion
        for (int i = 0; i < questButtons.Length && i < questManager.questCompleted.Length; i++)
        {
            if (questManager.questCompleted[i])
            {
                questButtons[i].gameObject.SetActive(false); // Hide button for completed quests
            }
        }
    }
}
