using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader; // Handles joystick input
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject muzzleFlash;

    [Header("Settings")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float muzzleFlashDuration;

    private bool isFiring; // Tracks if fire button is held
    private float fireCooldown; // Cooldown timer for firing

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        inputReader.PrimaryFireEvent += HandlePrimaryFire; // Subscribe to fire input
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        inputReader.PrimaryFireEvent -= HandlePrimaryFire; // Unsubscribe from fire input
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Decrease the fire cooldown over time
        fireCooldown -= Time.deltaTime;

        if (isFiring && fireCooldown <= 0f)
        {
            FireProjectile(); // Trigger firing
            fireCooldown = 1 / fireRate; // Reset cooldown based on fire rate
        }
    }

    private void HandlePrimaryFire(bool firing)
    {
        isFiring = firing; // Set firing state based on button press
    }

    private void FireProjectile()
    {
        // Get aim direction from joystick input
        Vector2 aimDirection = inputReader.AimPosition;
        Debug.Log($"Firing Direction: {aimDirection}");

        if (aimDirection.magnitude < 0.1f) return; // Ignore minimal joystick input

        // Normalize aim direction
        Vector3 direction = new Vector3(aimDirection.x, aimDirection.y, 0).normalized;

        // Fire on the server
        PrimaryFireServerRpc(projectileSpawnPoint.position, direction);

        // Simulate locally
        SpawnDummyProjectile(projectileSpawnPoint.position, direction);
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        GameObject projectileInstance = Instantiate(serverProjectilePrefab, spawnPos, Quaternion.identity);
        projectileInstance.transform.up = direction;

        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.linearVelocity = direction * projectileSpeed; // Set projectile speed
        }
    }

    private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction)
    {
        muzzleFlash.SetActive(true);

        GameObject projectileInstance = Instantiate(clientProjectilePrefab, spawnPos, Quaternion.identity);
        projectileInstance.transform.up = direction;

        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.linearVelocity = direction * projectileSpeed; // Set projectile speed
        }

        // Optional: Disable muzzle flash after a duration
        StartCoroutine(DisableMuzzleFlashAfterDuration());
    }

    private IEnumerator DisableMuzzleFlashAfterDuration()
    {
        yield return new WaitForSeconds(muzzleFlashDuration);
        muzzleFlash.SetActive(false);
    }
}
