using System;
using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int startingHealth = 3;
    [SerializeField] private GameObject deathVFXPrefab;
    [SerializeField] private AudioClip enemyDieSFX;
    [SerializeField] private float knockBackThrust = 15f;
    [SerializeField] private int expAmount = 100;

    public string enemyQuestName;
    public event Action<int, int> OnHealthChanged; // Event for health changes

    private int currentHealth;
    private QuestManager theQM;
    private Knockback knockback;
    private Flash flash;

    public int StartingHealth => startingHealth; // Getter for starting health

    private void Awake()
    {
        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
    }

    private void Start()
    {
        currentHealth = startingHealth;
        theQM = FindFirstObjectByType<QuestManager>();
    }

    public void TakeDamage(int damage)
    {
        int oldHealth = currentHealth;
        currentHealth -= damage;

        // Invoke the health change event
        OnHealthChanged?.Invoke(oldHealth, currentHealth);

        knockback.GetKnockedBack(PlayerController.Instance.transform, knockBackThrust);
        StartCoroutine(flash.FlashRoutine());
        StartCoroutine(CheckDetectDeathRoutine());
    }

    private IEnumerator CheckDetectDeathRoutine()
    {
        yield return new WaitForSeconds(flash.GetRestoreMatTime());
        DetectDeath();
    }

    private void DetectDeath()
    {
        if (currentHealth <= 0)
        {
            Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            GetComponent<PickUpSpawner>().DropItems();
            theQM.enemyKilled = enemyQuestName;
            Experience.Instance.AddExperience(expAmount);
            SoundFXManager.instance.PlaySoundFXClip(enemyDieSFX, transform, 1f);
            Destroy(gameObject);
        }
    }
}
