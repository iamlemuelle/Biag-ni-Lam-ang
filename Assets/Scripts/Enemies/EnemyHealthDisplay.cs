using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public EnemyHealth enemyHealth;
    [SerializeField] public Image healthBarImage;

    private void Start()
    {
        // Initialize the health bar with the starting value
        UpdateHealthBar(enemyHealth.StartingHealth, enemyHealth.StartingHealth);

        // Subscribe to health changes
        enemyHealth.OnHealthChanged += UpdateHealthBar;
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        enemyHealth.OnHealthChanged -= UpdateHealthBar;
    }

    // Update the health bar based on the new health value
    private void UpdateHealthBar(int oldHealth, int newHealth)
    {
        healthBarImage.fillAmount = (float)newHealth / enemyHealth.StartingHealth;
    }
}
