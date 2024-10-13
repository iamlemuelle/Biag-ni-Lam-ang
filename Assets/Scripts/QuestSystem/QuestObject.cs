using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestObject : MonoBehaviour
{
    [SerializeField] private int expAmount;
    public int questNumber;
    public QuestManager theQM;

    // Fields for quest description and rewards
    [Header("Quest Details")]
    [SerializeField] private string questDescription; 
    [SerializeField] private string questReward;

    // Dialogue arrays for different languages
    [Header("Start Text")]
    [SerializeField] private string[] startTextTagalog;
    [SerializeField] private string[] startTextIlocano;

    [Header("End Text")]
    [SerializeField] private string[] endTextTagalog;
    [SerializeField] private string[] endTextIlocano;

    // Combined character image for both Tagalog and Ilocano
    [Header("Character Image")]
    [SerializeField] private Sprite characterImage;

    public bool isItemQuest;
    public string targetItem;
    public bool isEnemyQuest;
    public string targetEnemy;
    public int enemiesToKill;
    private int enemyKillCount;

    [Header("Reward System")]
    [SerializeField] private GameObject rewardPrefab; // Prefab for the reward item
    [SerializeField] private Transform rewardSpawnPoint; // Where to spawn the reward

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (isItemQuest)
        {
            if (theQM.itemCollected == targetItem)
            {
                theQM.itemCollected = null;
                EndQuest();
            }
        }

        if (isEnemyQuest)
        {
            if (theQM.enemyKilled == targetEnemy)
            {
                theQM.enemyKilled = null;
                enemyKillCount++;
            }

            if (enemyKillCount >= enemiesToKill)
            {
                EndQuest();
            }
        }
    }

    public void StartQuest()
    {
        string currentLanguage = LanguageManager.Instance.currentLanguage;

        if (currentLanguage == "tag")
        {
            theQM.ShowQuestText(startTextTagalog);
        }
        else if (currentLanguage == "il")
        {
            theQM.ShowQuestText(startTextIlocano);
        }
        else
        {
            Debug.LogError("No dialogue available for language: " + currentLanguage);
        }

        theQM.ShowCharacterImage(characterImage); // Display the same character image for both languages
    }

    public void EndQuest()
    {
        if (!theQM.questCompleted[questNumber])  // Ensure quest is only completed once
        {
            string currentLanguage = LanguageManager.Instance.currentLanguage;

            if (currentLanguage == "tag")
            {
                theQM.ShowQuestText(endTextTagalog);
            }
            else if (currentLanguage == "il")
            {
                theQM.ShowQuestText(endTextIlocano);
            }
            else
            {
                Debug.LogError("No dialogue available for language: " + currentLanguage);
            }

            theQM.questCompleted[questNumber] = true;
            Experience.Instance.AddExperience(expAmount);
            Debug.Log("Quest " + questNumber + " has been completed.");

            // Trigger the QuestManager to handle completed quests
            theQM.QuestCompleted();

            // Update quest log
            theQM.UpdateQuestLog();

            // Instantiate reward if available
            InstantiateReward();
        }
    }

    private void InstantiateReward()
    {
        // Check if a reward is set before instantiating
        if (rewardPrefab != null)
        {
            if (rewardSpawnPoint != null)
            {
                Instantiate(rewardPrefab, rewardSpawnPoint.position, rewardSpawnPoint.rotation);
                Debug.Log("Reward instantiated: " + rewardPrefab.name);
            }
            else
            {
                Debug.LogWarning("RewardSpawnPoint is not set.");
            }
        }
        else
        {
            Debug.Log("No reward set for this quest. Skipping reward instantiation.");
        }

        // Alternatively, update player's gold or other in-game resources as a reward
        if (questReward == "Gold")
        {
            EconomyManager.Instance.UpdateCurrentGold();
        }
        else if (questReward == "Item")
        {
            // InventoryManager.Instance.AddItemToInventory("RewardItem");
            Debug.Log("Added to your item");
        }
    }

    public string GetDescription()
    {
        return questDescription;
    }

    public string GetReward()
    {
        return questReward;
    }
}
