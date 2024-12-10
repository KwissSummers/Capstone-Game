using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;               // Reference to the player
    public float smoothTime = 0.1f;        // Smooth time for following
    private Vector3 velocity = Vector3.zero;

    [Header("Room Bounds")]
    public Transform bottomLeft;           // Bottom-left corner of the room
    public Transform topRight;             // Top-right corner of the room

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (player == null)
        {
            Debug.LogError("Player reference is missing!");
        }
    }

    private void LateUpdate()
    {
        if (player == null) return;

        // Smoothly follow the player
        Vector3 targetPosition = player.position;

        // Clamp camera position to room bounds
        float horizontalExtent = cam.orthographicSize * Screen.width / Screen.height;
        float verticalExtent = cam.orthographicSize;

        float minX = bottomLeft.position.x + horizontalExtent;
        float maxX = topRight.position.x - horizontalExtent;
        float minY = bottomLeft.position.y + verticalExtent;
        float maxY = topRight.position.y - verticalExtent;

        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        // Smoothly transition to the target position
        targetPosition.z = transform.position.z; // Keep the original Z position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
