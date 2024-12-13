using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 7f; // Base walk speed
    public float maxSpeedMultiplier = 1.5f; // Max speed multiplier
    public float speedIncreaseTime = 2f; // Time to maintain direction before speed increases
    private float currentSpeed; // Current speed based on time walking in the same direction
    private float timeWalkingInDirection = 0f; // Time spent walking in the same direction

    private float jumpSpeed = 20f; // Jump speed
    private float fallSpeed = 20f; // Maximum fall speed
    private int maxJumpSteps = 2; // Max number of jumps
    private float dashSpeed = 30f; // Speed of the dash
    private float dashCooldown = 0.5f; // Cooldown between dashes
    private float dashDuration = 0.25f; // Duration of the dash
    private int maxDashes = 2; // Max number of dashes allowed
    private float freefallGravScale = 8f;
    private bool facingLeft = false; // Is the player facing left

    [SerializeField] private Rigidbody2D rigidBody;

    private float camSmoothSpeed = 0.1f;
    private Vector3 velocity = Vector3.zero;

    [Header("Ground and Roof Checks")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform roofCheck;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
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

    private float healingTimer = 0f; // To track how long 'D' is held down for healing

    // New input disabling mechanism
    private bool inputDisabled = false; // Flag to disable all player inputs temporarily

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player");
        dashesRemaining = maxDashes; // Initialize dash count
    }

    private void Update()
    {
        if (inputDisabled) return; // Skip input handling if input is disabled
        GetInputs(); // Input management
    }

    private void FixedUpdate()
    {
        if (inputDisabled) return; // Skip movement updates if input is disabled
        HandleMovement(); // Movement, dashing and jumping updates
    }

    private void GetInputs()
    {
        // Horizontal movement using only left and right arrow keys
        xAxis = 0f; // Reset horizontal input every frame
        if (Input.GetKey(KeyCode.LeftArrow)) xAxis = -1f;
        else if (Input.GetKey(KeyCode.RightArrow)) xAxis = 1f;

        // Jump is bound to the Z key
        isJumping = Input.GetKey(KeyCode.Z);

        // Dash triggered by the 'C' key
        if (!isDashing && Input.GetKeyDown(KeyCode.C) && dashesRemaining > 0 && dashCooldownTimer <= 0)
        {
            isDashing = true;
        }

        // Healing triggered by holding 'D' for 1.5 seconds
        if (Input.GetKey(KeyCode.D))
        {
            healingTimer += Time.deltaTime;

            // Start healing when the timer reaches 1.5 seconds and healing isn't already in progress
            if (healingTimer >= 1.5f)
            {
                HealthManager healthManager = GetComponent<HealthManager>();
                StaminaManager staminaManager = GetComponent<StaminaManager>();

                if (healthManager != null && staminaManager != null)
                {
                    healthManager.Heal(0.25f * healthManager.maxHealth, staminaManager); // Heal 25% of max health
                }

                healingTimer = 0f; // Reset the healing timer after the heal has triggered
            }
        }
        else
        {
            healingTimer = 0f; // Reset the healing timer if the 'D' key is released
        }
    }

    private void HandleMovement()
    {
        if (!isDashing && !recoilingX) // Prevent movement during dashing or recoiling
        {
            // Check if the player has been walking in the same direction long enough
            if (Mathf.Abs(xAxis) > 0)
            {
                timeWalkingInDirection += Time.deltaTime; // Increment time walking in direction

                // Gradually increase speed over time
                if (timeWalkingInDirection >= speedIncreaseTime)
                {
                    currentSpeed = Mathf.Lerp(walkSpeed, walkSpeed * maxSpeedMultiplier, (timeWalkingInDirection - speedIncreaseTime) / speedIncreaseTime);
                }
            }
            else
            {
                ResetSpeed(); // Reset speed if the player is not moving
            }

            // Apply movement with the current speed
            rb.velocity = new Vector2(xAxis * currentSpeed, rb.velocity.y);
            Flip();
        }
        else if (recoilingX)
        {
            ApplyRecoilX(); // Apply recoil during recoil state
        }

        HandleJump(); // Handle jumps
        HandleDash(); // Handle dashing
    }

    private void ApplyRecoilX()
    {
        // Adjust recoil speed and direction based on the attack's recoil
        float recoilXSpeed = 45f;
        float recoilDuration = 0.5f;
        if (facingLeft) // Facing left
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

    private void ResetSpeed()
    {
        // Reset the current speed to the original walkSpeed
        currentSpeed = walkSpeed;
        timeWalkingInDirection = 0f; // Reset the time spent walking in the same direction
    }

    private void HandleJump()
    {
        if (!isDashing)
        {
            if (jumpSteps < maxJumpSteps && (isJumping && !wasJumping))
            {
                // The Z key is initially pressed down, initiate the jump.
                rb.velocity = new Vector2(rb.velocity.x, jumpSpeed); // Apply the initial jump speed
                SetGravityScale(5f);
                jumpSteps++; // Increment the jump step counter.
            }

            if (!isJumping)
            {
                SetGravityScale(freefallGravScale);
            }
            else if (isJumping)
            {
                SetGravityScale(3f);
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
        SetGravityScale(0); // Disable gravity during dash
        Vector2 dashDirection = facingLeft ? Vector2.left : Vector2.right;
        rb.velocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        dashesRemaining--;
        dashCooldownTimer = dashCooldown;

        // Regenerate dash after cooldown
        yield return new WaitForSeconds(dashCooldown);
        dashesRemaining = Mathf.Min(dashesRemaining + 1, maxDashes);
    }

    private void Flip()
    {
        // Check the movement input to determine the facing direction
        if (xAxis > 0) // Moving right
        {
            if (facingLeft)
            {
                facingLeft = false;
                transform.localScale = new Vector3(1.5f, 2f, 1f); // Flip the player to face right
            }
        }
        else if (xAxis < 0) // Moving left
        {
            if (!facingLeft)
            {
                facingLeft = true;
                transform.localScale = new Vector3(-1.5f, 2f, 1f); // Flip the player to face left
            }
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

    public bool IsJumping() { return isJumping; }

    public bool IsFacingLeft() { return facingLeft; }

    private void SetGravityScale(float scale)
    {
        rigidBody.gravityScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            jumpSteps = 0;
            wasJumping = false;
        }
    }

    // New method to disable input temporarily
    public void DisableInput(float duration)
    {
        if (!inputDisabled)
        {
            StartCoroutine(DisableInputCoroutine(duration));
        }
    }

    private IEnumerator DisableInputCoroutine(float duration)
    {
        inputDisabled = true;
        yield return new WaitForSeconds(duration);
        inputDisabled = false;
    }
}
