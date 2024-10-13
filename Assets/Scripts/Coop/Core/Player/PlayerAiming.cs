using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader; // You can still use this if it's handling touch input in your own way.
    [SerializeField] private Transform weaponTransform;

    private void LateUpdate() {
        if (!IsOwner) { return; }

        AimWeapon();
    }

    private void AimWeapon() {
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);  // Get the first touch on the screen

            // Convert the touch position to world space
            Vector2 aimScreenPosition = touch.position;
            Vector2 aimWorldPosition = Camera.main.ScreenToWorldPoint(aimScreenPosition);

            // Rotate the weapon transform based on the touch position
            weaponTransform.up = new Vector2(
                aimWorldPosition.x - weaponTransform.position.x,
                aimWorldPosition.y - weaponTransform.position.y);
        }
    }
}
