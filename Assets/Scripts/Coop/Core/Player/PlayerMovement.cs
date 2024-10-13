using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer mySpriteRenderer;
    [SerializeField] private TrailRenderer myTrailRenderer; // For dash trail effect

    [Header("Settings")]
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float dashSpeed = 8f; // Dash speed multiplier
    [SerializeField] private float dashDuration = 0.2f; // Duration of dash
    [SerializeField] private float dashCooldown = 0.25f; // Cooldown between dashes
    [SerializeField] private AudioClip dashSFX; // Optional sound effect for dashing

    private Vector2 previousMovementInput;
    private bool facingLeft;
    private bool isDashing = false;
    private bool canDash = true; // Controls if the player can dash

    private static PlayerMovement instance;

    public static PlayerMovement Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<PlayerMovement>();
                if (instance == null) {
                    GameObject singleton = new GameObject(nameof(PlayerMovement));
                    instance = singleton.AddComponent<PlayerMovement>();
                }
            }
            return instance;
        }
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }

        rb = GetComponent<Rigidbody2D>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        if (!IsOwner) { return; }

        inputReader.MoveEvent += HandleMove;
        inputReader.DashEvent += HandleDash; // Subscribe to dash event
    }

    public override void OnNetworkDespawn() {
        base.OnNetworkDespawn();

        if (!IsOwner) { return; }

        inputReader.MoveEvent -= HandleMove;
        inputReader.DashEvent -= HandleDash; // Unsubscribe from dash event
    }

    private void FixedUpdate() {
        if (!IsOwner || isDashing) { return; }

        Move();
        AdjustPlayerFacingDirection();
    }

    private void Move() {
        rb.velocity = previousMovementInput * movementSpeed;
    }

    private void HandleMove(Vector2 movementInput) {
        previousMovementInput = movementInput;
    }

    private void AdjustPlayerFacingDirection() {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);
        mySpriteRenderer.flipX = mousePos.x < playerScreenPoint.x;
        facingLeft = mySpriteRenderer.flipX;
    }

    private void HandleDash() {
        if (canDash && !isDashing) {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash() {
        isDashing = true;
        canDash = false;

        // Optional: Play dash sound effect
        if (dashSFX != null) {
            AudioSource.PlayClipAtPoint(dashSFX, transform.position);
        }

        // Optional: Enable trail effect during dash
        if (myTrailRenderer != null) {
            myTrailRenderer.emitting = true;
        }

        // Temporarily increase the movement speed
        float originalSpeed = movementSpeed;
        movementSpeed *= dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        // Reset movement speed and stop dash
        movementSpeed = originalSpeed;
        isDashing = false;

        // Optional: Disable trail effect after dash
        if (myTrailRenderer != null) {
            myTrailRenderer.emitting = false;
        }

        yield return new WaitForSeconds(dashCooldown);

        // Allow dashing again after cooldown
        canDash = true;
    }
}
