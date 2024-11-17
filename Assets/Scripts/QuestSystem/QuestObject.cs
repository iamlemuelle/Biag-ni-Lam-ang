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
        string[] dialogue = currentLanguage == "tag" ? startTextTagalog : startTextIlocano;
        theQM.ShowQuestText(dialogue);
        theQM.ShowCharacterImage(characterImage);
    }

    public void EndQuest()
    {
        if (!theQM.questCompleted[questNumber])
        {
            string currentLanguage = LanguageManager.Instance.currentLanguage;
            string[] dialogue = currentLanguage == "tag" ? endTextTagalog : endTextIlocano;

            theQM.ShowQuestText(dialogue);
            theQM.questCompleted[questNumber] = true;
            Experience.Instance.AddExperience(expAmount);
            theQM.QuestCompleted();
            theQM.UpdateQuestLog();
            InstantiateReward();
        }
    }

    private void InstantiateReward()
    {
        if (rewardPrefab != null && rewardSpawnPoint != null)
        {
            Instantiate(rewardPrefab, rewardSpawnPoint.position, rewardSpawnPoint.rotation);
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