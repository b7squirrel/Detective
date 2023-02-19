using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemPickUpObject : Collectable, IPickUpObject
{
    [field: SerializeField] public int ExpAmount { get; set; }
    public void OnPickUp(Character character)
    {
        if(ExpAmount == 0) ExpAmount = 400;
        character.level.AddExperience(ExpAmount);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + (dir * 6f));
    }
}
