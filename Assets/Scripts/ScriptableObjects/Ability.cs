using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "New Ability")]
public class Ability : ScriptableObject
{
    public GameObject attackPrefab;     //prefab with animations and sound
    public float damage;                //damage ability does
    public float cooldown;              //cooldown before 
    public float lifespan;              //how long object lasts for
    public float speed;                 //speed the object travels
    public float spawnDistance;         //Distance from the player's origin point
}
