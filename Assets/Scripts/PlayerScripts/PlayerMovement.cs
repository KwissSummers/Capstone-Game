using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControllerScript : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 7f; // Base walk speed
    public float jumpSpeed = 80f; // Jump speed
    public float fallSpeed = 18f; // Maximum fall speed
    public int maxJumpSteps = 2; // Max number of jumps
    public float dashSpeed = 20f; // Speed of the dash
    public float dashCooldown = 0.5f; // Cooldown between dashes
    public float dashDuration = 0.2f; // Duration of the dash
    public int maxDashes = 2; // Max number of dashes allowed

    [SerializeField] private Rigidbody2D rigidBody;

    private float camSmoothSpeed = 0.1f;
    private Vector3 velocity = Vector3.zero;

    [Header("Ground and Roof Checks")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform roofCheck;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private new Camera camera;
    private GameObject player;

    private float xAxis; // Horizontal input axis
    public bool isJumping; // Is the jump button held?
    public bool wasJumping; // Was the player jumping in the last frame?
    private int jumpSteps; // Number of jumps used
    private bool isDashing; // Is the player dashing?
    private float dashCooldownTimer; // Timer for dash cooldown
    private int dashesRemaining; // Number of dashes left

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        camera = Camera.main;
        player = GameObject.FindWithTag("Player");
        dashesRemaining = maxDashes; // Initialize dash count
    }

    private void Update()
    {
        GetInputs();
        HandleMovement();
        HandleJump();
        HandleDash(); // Handle dashing
    }

    private void LateUpdate()
    {
        camera.transform.position = Vector3.SmoothDamp(
            camera.transform.position,
            new Vector3(player.transform.position.x, player.transform.position.y, camera.transform.position.z) + new Vector3(0, 1, 0),
            ref velocity,
            camSmoothSpeed
        );
    }

    private void GetInputs()
    {
        xAxis = Input.GetAxis("Horizontal");
        isJumping = Input.GetButton("Jump");

        if (Input.GetButtonDown("Dash") && dashesRemaining > 0 && dashCooldownTimer <= 0)
        {
            isDashing = true;
        }
    }

    private void HandleMovement()
    {
        // Prevent movement during dashing
        if (!isDashing)
        {
            rb.velocity = new Vector2(xAxis * walkSpeed, rb.velocity.y);
            Flip();
        }
    }

    private void HandleJump()
    {
        if (jumpSteps < maxJumpSteps)
        {
            if (isJumping && !wasJumping)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
                jumpSteps++;
            }
        }

        if (isJumping && wasJumping)
        {
            rigidBody.gravityScale = 3;
        }
        else
        {
            rigidBody.gravityScale = 6;
        }

        if (rb.velocity.y < -fallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -fallSpeed);
        }

        wasJumping = isJumping;
    }

    private void HandleDash()
    {
        if (isDashing)
        {
            StartCoroutine(Dash());
        }

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        float originalGravity = rigidBody.gravityScale;
        rigidBody.gravityScale = 0; // Disable gravity during dash
        Vector2 dashDirection = new Vector2(xAxis, 0).normalized;
        rb.velocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rigidBody.gravityScale = originalGravity;
        isDashing = false;
        dashesRemaining--;
        dashCooldownTimer = dashCooldown;

        // Regenerate dash after cooldown
        yield return new WaitForSeconds(dashCooldown);
        dashesRemaining = Mathf.Min(dashesRemaining + 1, maxDashes);
    }

    private void Flip()
    {
        if (xAxis > 0)
        {
            transform.localScale = new Vector3(1, 1.5f, 1);
        }
        else if (xAxis < 0)
        {
            transform.localScale = new Vector3(-1, 1.5f, 1);
        }
    }

    private bool Grounded()
    {
        return Physics2D.Raycast(groundCheck.position, Vector2.down, 0.1f, groundLayer);
    }

    private bool Roofed()
    {
        return Physics2D.Raycast(roofCheck.position, Vector2.up, 0.1f, groundLayer);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            jumpSteps = 0;
            wasJumping = false;
        }
    }

    public bool IsJumping() { return isJumping; }
}
