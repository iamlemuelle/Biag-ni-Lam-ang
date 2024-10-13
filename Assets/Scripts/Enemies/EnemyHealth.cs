using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int startingHealth = 3;
    [SerializeField] private GameObject deathVFXPrefab;
    [SerializeField] private AudioClip enemyDieSFX;
    [SerializeField] private float knockBackThrust = 15f;
    [SerializeField] private int expAmount = 100;
    public string enemyQuestName;
    private QuestManager theQM;

    private int currentHealth;
    private Knockback knockback;
    private Flash flash;

    private void Awake() {
        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
    }
    private void Start() {
        currentHealth = startingHealth;
        theQM = FindObjectOfType<QuestManager>();
    }

    public void TakeDamage(int damage) {
        currentHealth -= damage;
        knockback.GetKnockedBack(PlayerController.Instance.transform, knockBackThrust);
        StartCoroutine(flash.FlashRoutine());
        StartCoroutine(CheckDetectDeathRoutine());
    }

    private IEnumerator CheckDetectDeathRoutine() {
        yield return new WaitForSeconds(flash.GetRestoreMatTime());
        DetectDeath();
    }

    private void DetectDeath() {
        if (currentHealth <= 0) {
            Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            GetComponent<PickUpSpawner>().DropItems();
            theQM.enemyKilled = enemyQuestName;
            Experience.Instance.AddExperience(expAmount);
            SoundFXManager.instance.PlaySoundFXClip(enemyDieSFX, transform, 1f); // PLAY SOUNDFX
            Destroy(gameObject);
        }
    }
}
