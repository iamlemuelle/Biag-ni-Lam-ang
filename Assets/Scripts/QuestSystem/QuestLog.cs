using System.Collections.Generic;
using UnityEngine;

public class QuestLog : MonoBehaviour
{
    [SerializeField] private Transform questListParent; // Parent object to hold quest UI elements
    [SerializeField] private GameObject questUIPrefab;  // Prefab for displaying individual quests

    private Dictionary<int, GameObject> activeQuestUIs = new Dictionary<int, GameObject>();

    public void UpdateQuestLog(QuestManager questManager)
    {
        foreach (var quest in questManager.quests)
        {
            int questNumber = quest.questNumber;

            if (questManager.questCompleted[questNumber])
            {
                // Remove completed quests from the log
                if (activeQuestUIs.ContainsKey(questNumber))
                {
                    Destroy(activeQuestUIs[questNumber]);
                    activeQuestUIs.Remove(questNumber);
                }
            }
            else
            {
                // Display active quests
                if (!activeQuestUIs.ContainsKey(questNumber))
                {
                    GameObject questUI = Instantiate(questUIPrefab, questListParent);
                    questUI.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = quest.GetDescription();
                    activeQuestUIs[questNumber] = questUI;
                }
            }
        }
    }
}
