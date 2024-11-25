using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader; // Joystick input
    [SerializeField] private Transform weaponTransform;

    private void LateUpdate() {
        if (!IsOwner) { return; }

        AimWeapon();
    }

    private void AimWeapon() {
        // Get joystick input for aiming direction
        Vector2 aimDirection = inputReader.AimPosition;

        if (aimDirection.magnitude > 0.1f) { // Ignore small input to prevent jitter
            // Rotate the weapon transform to face the direction of the joystick
            weaponTransform.up = aimDirection;
        }
    }
}
