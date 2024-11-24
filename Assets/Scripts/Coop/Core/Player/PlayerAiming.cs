using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform weaponTransform;

    private void OnEnable()
    {
        if (!IsOwner) return;
        inputReader.AimEvent += HandleAim;
    }

    private void OnDisable()
    {
        if (!IsOwner) return;
        inputReader.AimEvent -= HandleAim;
    }

    private void HandleAim(Vector2 aimDirection)
    {
        Debug.Log($"Handling Aim: {aimDirection}");
        if (aimDirection.magnitude > 0.1f) // Ignore slight joystick drift
        {
            weaponTransform.up = aimDirection; // Rotate weapon based on joystick direction
            Debug.Log($"Weapon Up: {weaponTransform.up}");
        }
    }
}
