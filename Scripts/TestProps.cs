using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestProps : MonoBehaviour, Idamageable
{
    public void TakeDamage(int damage, float knockBackChance, Vector2 target)
    {
        Debug.Log("hit");
    }
}
