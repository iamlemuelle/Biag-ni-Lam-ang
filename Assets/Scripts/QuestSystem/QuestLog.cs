using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestLog : MonoBehaviour
{
    public GameObject questItemPrefab; // Drag your QuestItemPrefab here in the Inspector
    public Transform contentPanel; // This should be assigned to the Content object of your Scroll View
    private QuestManager theQM;

    void Start()
    {
        theQM = FindObjectOfType<QuestManager>();
        if (theQM == null)
        {
            Debug.LogError("QuestManager not found in the scene. Please ensure it's added.");
            return;
        }
        
        UpdateQuestLog();
    }

    public void UpdateQuestLog()
    {
        // Clear existing quest items
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Loop through quests and create UI elements
        for (int i = 0; i < theQM.quests.Length; i++)
        {
            QuestObject quest = theQM.quests[i];

            if (quest == null)
            {
                Debug.LogError($"Quest at index {i} is null. Ensure it's assigned in the Inspector.");
                continue; // Skip to the next iteration if the quest is null
            }

            if (!theQM.questCompleted[i]) // Only show active quests
            {
                // Instantiate the quest item prefab
                GameObject newQuestItem = Instantiate(questItemPrefab, contentPanel);

                // Check for the required text components
                var questNameText = newQuestItem.transform.Find("QuestNameText")?.GetComponent<Text>();
                var questDescriptionText = newQuestItem.transform.Find("QuestDescriptionText")?.GetComponent<Text>();
                var questRewardText = newQuestItem.transform.Find("QuestRewardText")?.GetComponent<Text>();

                if (questNameText == null || questDescriptionText == null || questRewardText == null)
                {
                    Debug.LogError($"Missing text component in QuestItemPrefab for quest: {quest.questNumber}");
                }
                else
                {
                    // Set the quest item texts
                    questNameText.text = "Quest " + quest.questNumber; // or use a quest name if available
                    questDescriptionText.text = quest.GetDescription();
                    questRewardText.text = quest.GetReward();
                }
            }
        }
    }
}
