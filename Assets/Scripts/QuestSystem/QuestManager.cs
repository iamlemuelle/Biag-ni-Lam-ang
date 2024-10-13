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

    private int completedQuestCount = 0;  // Track how many quests are completed
    private Transform playerTransform;  // Reference to the player's transform

    [Header("Reward System")]
    [SerializeField] private GameObject[] rewardPrefabs; // Array of possible reward prefabs
    [SerializeField] private Transform rewardSpawnPoint; // Where to spawn the reward
    [SerializeField] private AnimationCurve animCurve;  // Animation curve for spawn
    [SerializeField] private float heightY = 1.5f;  // Max height for the spawn animation
    [SerializeField] private float popDuration = 1f;  // Duration for the spawn animation

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (theDM == null)
        {
            theDM = FindObjectOfType<DialogueManager>();
        }

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;  // Find player by tag

        questCompleted = new bool[quests.Length];

        // Dynamically assign quests if they haven't been assigned in the Inspector
        if (quests.Length == 0)
        {
            quests = FindObjectsOfType<QuestObject>();

            for (int i = 0; i < quests.Length; i++)
            {
                quests[i].theQM = this;  // Make sure the QuestManager is linked
            }
        }
    }

    public void ShowQuestText(string[] questText)
    {
        if (theDM != null)
        {
            theDM.dialogLines = questText;  // Assign the array of dialogues
            theDM.currentLine = 0;
            theDM.ShowDialogue();
        }
        else
        {
            Debug.LogError("DialogueManager (theDM) is not assigned in QuestManager.");
        }
    }

    public void ShowCharacterImage(Sprite characterImage)
    {
        if (characterImage != null)
        {
            characterImageUI.sprite = characterImage;
            characterImageUI.gameObject.SetActive(true);  // Make the image visible
        }
        else
        {
            characterImageUI.gameObject.SetActive(false); // Hide the image if none is available
        }
    }

    public void UpdateQuestLog()
    {
        // Logic to update the quest log UI
        FindObjectOfType<QuestLog>().UpdateQuestLog();
    }

    public void QuestCompleted()
    {
        completedQuestCount++;

        if (completedQuestCount % 2 == 0) // Every 2 quests, instantiate a random reward
        {
            InstantiateRandomReward();
        }
    }

    private void InstantiateRandomReward()
    {
        if (rewardPrefabs != null && rewardPrefabs.Length > 0)
        {
            // Select a random reward from the array
            GameObject randomReward = rewardPrefabs[Random.Range(0, rewardPrefabs.Length)];

            // Update the rewardSpawnPoint to the player's position
            rewardSpawnPoint.position = playerTransform.position;

            // Instantiate the reward and start the animation coroutine
            GameObject reward = Instantiate(randomReward, rewardSpawnPoint.position, Quaternion.identity);
            StartCoroutine(AnimCurveSpawnRoutine(reward.transform));
            Debug.Log("Random reward instantiated for completing quests!");
        }
        else
        {
            Debug.LogWarning("RewardPrefabs is not set.");
        }
    }

    private IEnumerator AnimCurveSpawnRoutine(Transform reward)
    {
        Vector2 startPoint = reward.position;
        float randomX = reward.position.x + Random.Range(-2f, 2f);
        float randomY = reward.position.y + Random.Range(-1f, 1f);

        Vector2 endPoint = new Vector2(randomX, randomY);
        float timePassed = 0f;

        while (timePassed < popDuration)
        {
            timePassed += Time.deltaTime;
            float linearT = timePassed / popDuration;
            float heightT = animCurve.Evaluate(linearT);
            float height = Mathf.Lerp(0f, heightY, heightT);

            reward.position = Vector2.Lerp(startPoint, endPoint, linearT) + new Vector2(0f, height);
            yield return null;
        }
    }
}
