using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private GameObject enemyPrefab;  // Enemy prefab to spawn
    [SerializeField] private Transform[] spawnPoints; // List of spawn points
    [SerializeField] private float spawnInterval = 60f; // Time between spawns (60 seconds = 1 minute)
    [SerializeField] private int maxEnemiesPerSpawnPoint = 5;  // Max number of enemies for each spawn point

    private float timer;  // Timer for spawning enemies
    private int[] enemiesAtSpawnPoint;  // Track enemies spawned at each spawn point

    void Start() {
        timer = spawnInterval;  // Initialize the timer
        enemiesAtSpawnPoint = new int[spawnPoints.Length];  // Initialize array to track enemies at each spawn point
    }

    void Update() {
        timer -= Time.deltaTime;  // Decrease the timer every frame

        if (timer <= 0f) {
            SpawnEnemies();  // Spawn enemies when the timer reaches 0
            timer = spawnInterval;  // Reset the timer
        }
    }

    void SpawnEnemies() {
        for (int i = 0; i < spawnPoints.Length; i++) {
            if (enemiesAtSpawnPoint[i] < maxEnemiesPerSpawnPoint) {
                // If the spawn point has not reached the max number of enemies, spawn an enemy
                Instantiate(enemyPrefab, spawnPoints[i].position, spawnPoints[i].rotation);

                // Increment the count of spawned enemies at this spawn point
                enemiesAtSpawnPoint[i]++;
            }
        }
    }
}
