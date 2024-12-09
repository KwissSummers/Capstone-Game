using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Ability")]
public class Ability : ScriptableObject
{
    public GameObject attackPrefab;
    public float damage;
    public float cooldown;
    public float lifespan;
    public float speed;
}
