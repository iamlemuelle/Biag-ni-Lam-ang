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
    [SerializeField] private TrailRenderer myTrailRenderer; // For dash effect

    [Header("Settings")]
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float dashSpeed = 8f; // Dash speed multiplier
    [SerializeField] private float dashDuration = 0.2f; // Dash duration
    [SerializeField] private float dashCooldown = 0.25f; // Cooldown between dashes
    [SerializeField] private AudioClip dashSFX; // Optional dash sound

    private Animator myAnimator;
    private Vector2 previousMovementInput;
    private bool facingLeft;
    private bool isDashing = false;
    private bool canDash = true; // Controls dash availability

    // Removed singleton pattern from here to avoid conflicts during respawn

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) return;

        inputReader.MoveEvent += HandleMove;
        inputReader.DashEvent += HandleDash; // Subscribe to dash event
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner) return;

        inputReader.MoveEvent -= HandleMove;
        inputReader.DashEvent -= HandleDash; // Unsubscribe from dash event
    }

    private void FixedUpdate()
    {
        if (!IsOwner || isDashing) return;

        Move();
        AdjustPlayerFacingDirection();
    }

    private void Move()
    {
        rb.linearVelocity = previousMovementInput * movementSpeed;
    }

    private void HandleMove(Vector2 movementInput)
    {
        previousMovementInput = movementInput;

        myAnimator.SetFloat("moveX", movementInput.x);
        myAnimator.SetFloat("moveY", movementInput.y);
    }

    private void AdjustPlayerFacingDirection()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);
        mySpriteRenderer.flipX = mousePos.x < playerScreenPoint.x;
        facingLeft = mySpriteRenderer.flipX;
    }

    private void HandleDash()
    {
        if (canDash && !isDashing)
        {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        // Optional: Play dash sound effect
        if (dashSFX != null)
        {
            AudioSource.PlayClipAtPoint(dashSFX, transform.position);
        }

        // Optional: Enable trail effect during dash
        if (myTrailRenderer != null)
        {
            myTrailRenderer.emitting = true;
        }

        float originalSpeed = movementSpeed;
        movementSpeed *= dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        movementSpeed = originalSpeed;
        isDashing = false;

        // Disable trail effect after dash
        if (myTrailRenderer != null)
        {
            myTrailRenderer.emitting = false;
        }

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
