using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceTracker : MonoBehaviour
{
    private Transform player;
    private Transform boss;
    public float distance; // Distance between the player and the boss

    private void Start()
    {
        // Find the player and boss in the scene
        player = GameObject.FindWithTag("Player").transform;
        boss = GameObject.FindWithTag("Boss").transform;
    }

    private void Update()
    {
        if (player != null && boss != null)
        {
            // Calculate distance
            distance = Vector3.Distance(player.position, boss.position);
            Debug.Log($"Distance between Player and Boss: {distance}");
        }
    }
}
