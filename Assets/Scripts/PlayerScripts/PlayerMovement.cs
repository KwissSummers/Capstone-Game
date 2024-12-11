using System;
using System.Collections;
using System.Collections.Generic;
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
    private float lastXAxis = 0f; // Last horizontal input to detect direction change

    public float jumpSpeed = 18f; // Jump speed
    public float fallSpeed = 20f; // Maximum fall speed
    public int maxJumpSteps = 2; // Max number of jumps
    public float dashSpeed = 20f; // Speed of the dash
    public float dashCooldown = 0.5f; // Cooldown between dashes
    public float dashDuration = 0.5f; // Duration of the dash
    public int maxDashes = 2; // Max number of dashes allowed
    public float freefallGravScale = 8f;
    private bool isAtMaxHeight; // Whether the player has reached the maximum jump height.
    private bool isJumpingHeld = false; // To track if the player is holding the jump button.
    public bool facingLeft = false; // Is the player facing left

    [SerializeField] private Rigidbody2D rigidBody;

    private float camSmoothSpeed = 0.1f;
    private Vector3 velocity = Vector3.zero;

    [Header("Ground and Roof Checks")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform roofCheck;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    //private new Camera camera;
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
        //camera = Camera.main;
        player = GameObject.FindWithTag("Player");
        dashesRemaining = maxDashes; // Initialize dash count
    }

    private void Update()
    {
        GetInputs(); // Input management
    }

    private void FixedUpdate()
    {
        HandleMovement(); // Movement, dashing and jumping updates
    }

    //private void LateUpdate()
    //{
    //    camera.transform.position = Vector3.SmoothDamp(
    //        camera.transform.position,
    //        new Vector3(player.transform.position.x, player.transform.position.y, camera.transform.position.z) + new Vector3(0, 1, 0),
    //        ref velocity,
    //        camSmoothSpeed
    //    );
    //}

    private void GetInputs()
    {
        xAxis = Input.GetAxis("Horizontal");
        if(xAxis > 0)
        {
            facingLeft = false;
        }
        else if(xAxis < 0)
        {
            facingLeft = true;
        }
        isJumping = Input.GetButton("Jump");

        // Check if the 'C' key is pressed and the player can dash (dashesRemaining > 0 and dashCooldownTimer <= 0)
        if (!isDashing && Input.GetKeyDown(KeyCode.C) && dashesRemaining > 0 && dashCooldownTimer <= 0)
        {
            isDashing = true;
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
                 // The space bar is initially pressed down, initiate the jump.
                rb.velocity = new Vector2(rb.velocity.x, jumpSpeed); // Apply the initial jump speed
                SetGravityScale(5f);
                jumpSteps++; // Increment the jump step counter.
            }

            if (!isJumping)
            {
                SetGravityScale(freefallGravScale);
            } else if (isJumping)
            {
                SetGravityScale(7f);
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
        if (xAxis > 0)
        {
            transform.localScale = new Vector3(1.5f, 2f, 1f);
        }
        else if (xAxis < 0)
        {
            transform.localScale = new Vector3(-1.5f, 2f, 1f);
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

}
