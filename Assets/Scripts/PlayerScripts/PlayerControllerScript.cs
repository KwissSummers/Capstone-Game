using System.Collections; // Importing collections namespace for using collection classes.
using System.Collections.Generic; // Importing generic collections namespace.
using UnityEngine; // Importing UnityEngine namespace for Unity-specific classes.

public class PlayerControllerScript : MonoBehaviour // Inheriting from MonoBehaviour to use Unity's functionality.
{
    // Header attribute for organizing the Inspector UI in Unity.
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 7f; // Speed when walking, adjustable in Inspector.
    [SerializeField] private float jumpSpeed = 80f; // Vertical speed applied when the player jumps.
    [SerializeField] private float fallSpeed = 18f; // Maximum speed at which the player can fall.
    [SerializeField] private int maxJumpSteps = 2; // Maximum number of jumps the player can perform.
    [SerializeField] private float timeBetweenAttacks = 0.5f; // Cooldown time between attacks.
    [SerializeField] private Rigidbody2D rigidBody;

    private float camSmoothSpeed = 0.1f;
    private Vector3 velocity = Vector3.zero;

    [Header("Ground and Roof Checks")]
    [SerializeField] private Transform groundCheck; // Transform to determine the player's ground position.
    [SerializeField] private Transform roofCheck; // Transform to determine the player's roof position.
    [SerializeField] private LayerMask groundLayer; // Layer mask to identify ground objects for collision detection.

    private Rigidbody2D rb; // Rigidbody2D component for physics interactions.
    private Camera camera;
    private GameObject player;
    private float xAxis; // Variable to store horizontal input from the player.
    private bool isJumping; // Boolean to check if the player has initiated a jump.
    private bool wasJumping; // Was player jumping last frame?
    private int jumpSteps; // Counter for the number of jumps performed.
    private float timeSinceLastAttack; // Timer for tracking attack cooldown.
    private bool isAttacking; // Boolean to check if the player is currently attacking.

    // Called when the script instance is being loaded.
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component attached to this GameObject.
        camera = Camera.main;
        player = GameObject.FindWithTag("Player");
    }

    // Called once per frame.
    private void Update()
    {
        GetInputs(); // Get player input each frame.
        HandleMovement(); // Process movement logic.
        HandleJump(); // Process jump logic.
        HandleAttack(); // Process attack logic.


        //camera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, camera.transform.position.z) + new Vector3(0,2,0);
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

    // Method to gather player input.
    private void GetInputs()
    {
        xAxis = Input.GetAxis("Horizontal"); // Get horizontal input (A/D or Left/Right arrows).
        isJumping = Input.GetButton("Jump"); // Check if the jump button is pressed.
        

        // Check if the attack button is pressed and the player is not currently attacking.
        if (Input.GetButtonDown("Attack") && !isAttacking)
        {
            isAttacking = true; // Set attacking state to true.
        }
    }

    // Method to handle player movement.
    private void HandleMovement()
    {
        if (!isAttacking) // Only move if the player is not attacking.
        {
            // Set the Rigidbody's velocity based on horizontal input while maintaining vertical velocity.
            rb.velocity = new Vector2(xAxis * walkSpeed, rb.velocity.y);
            Flip(); // Call method to flip the player sprite based on movement direction.
        }
    }

    // Method to handle player jumping logic.
    private void HandleJump()
    {
        Debug.Log(isJumping);
        if (jumpSteps < maxJumpSteps) // Check if the player is grounded and can jump.
        {
            if (isJumping && !wasJumping) // If the player initiated a jump.
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpSpeed); // Apply jump speed to vertical velocity.
                jumpSteps++; // Increment jump count.
            }
        }

        if (isJumping && wasJumping)
        {
            rigidBody.gravityScale = 3;
        } else
        {
            rigidBody.gravityScale = 6;
        }

        // Limit the maximum falling speed to prevent unrealistic falls.
        if (rb.velocity.y < -fallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -fallSpeed); // Clamp the vertical velocity to the fall speed.
        }

        wasJumping = isJumping;
    }

    // Method to handle player attack logic.
    private void HandleAttack()
    {
        timeSinceLastAttack += Time.deltaTime; // Increment the attack timer.

        // Check if the player is attacking and the cooldown period has elapsed.
        if (isAttacking && timeSinceLastAttack >= timeBetweenAttacks)
        {
            timeSinceLastAttack = 0; // Reset attack timer.
            // Implement attack logic here (e.g., dealing damage, activating attack animation).
            isAttacking = false; // Reset attacking state after executing attack logic.
        }
    }

    // Method to flip the player sprite based on movement direction.
    private void Flip()
    {
        // Check horizontal input to determine facing direction.
        if (xAxis > 0) // If moving right.
        {
            transform.localScale = new Vector3(1, 1.5f, 1); // Face right.
        }
        else if (xAxis < 0) // If moving left.
        {
            transform.localScale = new Vector3(-1, 1.5f, 1); // Face left.
        }
    }

    // Method to check if the player is grounded using raycasting.
    private bool Grounded()
    {
        // Perform a raycast downwards from the groundCheck position.
        return Physics2D.Raycast(groundCheck.position, Vector2.down, 0.1f, groundLayer);
    }

    // Method to check if there’s a roof above the player using raycasting.
    private bool Roofed()
    {
        // Perform a raycast upwards from the roofCheck position.
        return Physics2D.Raycast(roofCheck.position, Vector2.up, 0.1f, groundLayer);
    }

    // Called when the collider enters a collision.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Reset jump steps when hitting the ground.
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")) // Check if collided with ground layer.
        {
            jumpSteps = 0; // Reset jump counter to allow jumping again.
            wasJumping = false;
        }
    }

    public bool IsJumping() { return isJumping; }
}
