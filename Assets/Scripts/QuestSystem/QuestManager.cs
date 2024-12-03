using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public QuestObject[] quests;
    public bool[] questCompleted;
    public DialogueManager theDM;
    public string itemCollected;
    public string enemyKilled;
    public Image characterImageUI;

    private int completedQuestCount = 0; // Track how many quests are completed
    private Transform playerTransform;   // Reference to the player's transform
    private QuestLog questLog;          // Reference to QuestLog

    [Header("Reward System")]
    [SerializeField] private GameObject[] rewardPrefabs; 
    [SerializeField] private Transform rewardSpawnPoint;
    [SerializeField] private AnimationCurve animCurve;
    [SerializeField] private float heightY = 1.5f;
    [SerializeField] private float popDuration = 1f;

    [Header("UI")]
    [SerializeField] private Button[] questButtons; // Array of buttons corresponding to quests

    void Awake()
    {
        DontDestroyOnLoad(gameObject); // Keep QuestManager across scenes
    }

    void Start()
    {
        InitializeQuestManager();
        questLog = FindObjectOfType<QuestLog>();
        questLog?.InitializeQuestLog(this);
    }

    private void OnEnable()
    {
        StartCoroutine(DelayedInitialization()); // Ensure references are valid after scene load
    }

    private IEnumerator DelayedInitialization()
    {
        yield return new WaitForSeconds(0.1f); // Slight delay to ensure everything is loaded
        InitializeQuestManager();
    }
    public void ShowQuestText(string[] questText)
    {
        if (theDM != null)
        {
            theDM.dialogLines = questText;
            theDM.currentLine = 0;
            theDM.ShowDialogue();
            Debug.Log("Showing quest text.");
        }
        else
        {
            Debug.LogError("DialogueManager not assigned.");
        }
    }

    public void ShowCharacterImage(Sprite characterImage)
    {
        if (characterImageUI != null)
        {
            characterImageUI.sprite = characterImage;
            characterImageUI.gameObject.SetActive(characterImage != null);
        }
    }

    private void InitializeQuestManager()
    {
        if (theDM == null)
        {
            theDM = FindFirstObjectByType<DialogueManager>(); // Find DialogueManager dynamically
        }

        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        questCompleted = new bool[quests.Length];

        if (quests.Length == 0)
        {
            quests = FindObjectsOfType<QuestObject>();
            foreach (var quest in quests)
            {
                quest.theQM = this; // Link QuestManager to each QuestObject
            }
        }

        completedQuestCount = PlayerPrefs.GetInt("CompletedQuestCount", 0);
    }
    public void CompleteQuest(int questNumber)
    {
        if (questNumber < questCompleted.Length && !questCompleted[questNumber])
        {
            questCompleted[questNumber] = true;

            // Notify the QuestLog to update the button visibility
            questLog?.UpdateQuestLog(this);

            UpdateQuestLog();
        }
    }

    public void UpdateQuestLog()
    {
        questLog?.UpdateQuestLog(this);
    }

    public void QuestCompleted()
    {
        completedQuestCount++;
        PlayerPrefs.SetInt("CompletedQuestCount", completedQuestCount); // Save to PlayerPrefs
        PlayerPrefs.Save(); // Ensure the data is saved immediately

        if (completedQuestCount % 5 == 0)
        {
            InstantiateRandomReward();
        }
    }

    private void InstantiateRandomReward()
    {
        if (rewardPrefabs != null && rewardPrefabs.Length > 0)
        {
            GameObject randomReward = rewardPrefabs[Random.Range(0, rewardPrefabs.Length)];
            rewardSpawnPoint.position = playerTransform.position;

            GameObject reward = Instantiate(randomReward, rewardSpawnPoint.position, Quaternion.identity);
            StartCoroutine(AnimCurveSpawnRoutine(reward.transform));
            Debug.Log("Random reward instantiated.");
        }
        else
        {
            Debug.LogWarning("No reward prefabs available.");
        }
    }

    private IEnumerator AnimCurveSpawnRoutine(Transform reward)
    {
        Vector2 startPoint = reward.position;
        Vector2 endPoint = startPoint + new Vector2(Random.Range(-2f, 2f), Random.Range(-1f, 1f));
        float timePassed = 0f;

        while (timePassed < popDuration)
        {
            timePassed += Time.deltaTime;
            float linearT = timePassed / popDuration;
            float heightT = animCurve.Evaluate(linearT);
            reward.position = Vector2.Lerp(startPoint, endPoint, linearT) + new Vector2(0f, Mathf.Lerp(0f, heightY, heightT));
            yield return null;
        }
    }
}
