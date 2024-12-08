using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 7f; // Base walk speed
    public float jumpSpeed = 80f; // Jump speed
    public float maxJumpHeightMultiplier = 2f; // Maximum jump height multiplier 
    public float fallSpeed = 18f; // Maximum fall speed
    public int maxJumpSteps = 2; // Max number of jumps
    public float dashSpeed = 20f; // Speed of the dash
    public float dashCooldown = 0.5f; // Cooldown between dashes
    public float dashDuration = 0.5f; // Duration of the dash
    public int maxDashes = 2; // Max number of dashes allowed
    private float jumpHoldTime = 0f; // Duration the spacebar has been held down.
    private bool isAtMaxHeight; // Whether the player has reached the maximum jump height.
    private bool isJumpingHeld = false; // To track if the player is holding the jump button.

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

    // Recoil variables
    public bool recoilingX = false; // Flag for horizontal recoil

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

        // Check if the 'C' key is pressed and the player can dash (dashesRemaining > 0 and dashCooldownTimer <= 0)
        if (Input.GetKeyDown(KeyCode.C) && dashesRemaining > 0 && dashCooldownTimer <= 0)
        {
            isDashing = true;
        }
    }

    private void HandleMovement()
    {
        if (!isDashing && !recoilingX) // Prevent movement during dashing or recoiling
        {
            rb.velocity = new Vector2(xAxis * walkSpeed, rb.velocity.y);
            Flip();
        }
        else if (recoilingX)
        {
            ApplyRecoilX(); // Apply recoil during recoil state
        }
    }

    private void ApplyRecoilX()
    {
        // Adjust recoil speed and direction based on the attack's recoil
        float recoilXSpeed = 45f;
        float recoilDuration = 0.5f;
        if (transform.localScale.x < 0) // Facing left
        {
            rb.velocity = new Vector2(-recoilXSpeed, rb.velocity.y);
        }
        else // Facing right
        {
            rb.velocity = new Vector2(recoilXSpeed, rb.velocity.y);
        }

        StartCoroutine(StopRecoilX(recoilDuration));
    }

    private IEnumerator StopRecoilX(float recoilDuration)
    {
        yield return new WaitForSeconds(recoilDuration);
        recoilingX = false; // Stop recoil after duration
    }

    private void HandleJump()
    {
        if (jumpSteps < maxJumpSteps)
        {
            if (isJumping && !wasJumping) // The space bar is initially pressed down, initiate the jump.
            {
                jumpHoldTime = 0f; // Reset jump hold time on new jump
                isAtMaxHeight = false; // Reset the max height flag
                rb.velocity = new Vector2(rb.velocity.x, jumpSpeed); // Apply the initial jump speed
                jumpSteps++; // Increment the jump step counter.
            }

            if (isJumping && !isAtMaxHeight) // The space bar is held down and we haven’t reached max height yet.
            {
                jumpHoldTime += Time.deltaTime; // Increase the hold time.
                float maxJumpHeight = jumpSpeed * maxJumpHeightMultiplier; // Calculate the max jump height.
                float currentJumpSpeed = Mathf.Lerp(jumpSpeed, maxJumpHeight, jumpHoldTime); // Gradually increase jump speed.

                if (rb.velocity.y < currentJumpSpeed) // Only increase the jump speed until the max height is reached.
                {
                    rb.velocity = new Vector2(rb.velocity.x, currentJumpSpeed); // Apply the updated jump speed.
                }

                if (rb.velocity.y >= maxJumpHeight) // Once the max jump height is reached, stop increasing speed.
                {
                    isAtMaxHeight = true; // Set the flag to true to stop increasing the jump height.
                    rb.velocity = new Vector2(rb.velocity.x, maxJumpHeight); // Ensure we cap the velocity at max height.
                }
            }
        }

        // As soon as the player releases the jump button or the max height is reached, gravity applies.
        if (!isJumping || isAtMaxHeight)
        {
            if (rb.velocity.y > 0) // Apply gravity only if the player is still going up.
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y - (fallSpeed * Time.deltaTime)); // Apply fall speed.
            }
        }

        // Handle gravity and fall speed
        if (rb.velocity.y < -fallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -fallSpeed); // Limit maximum fall speed.
        }

        wasJumping = isJumping; // Update jump status.
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
