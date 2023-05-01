using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableObject : MonoBehaviour, Idamageable
{
    public void TakeDamage(int damage, float knockBackChance, Vector2 target, GameObject hitEffect)
    {
        // knockBackChance값을 받아오지만 쓰지는 않는다
        gameObject.SetActive(false);
        GetComponent<DropOnDestroy>().CheckDrop();
    }
}
