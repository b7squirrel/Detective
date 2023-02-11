using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemPickUpObject : Collectable, IPickUpObject
{
    [SerializeField] private int expAmount;
    public void OnPickUp(Character character)
    {
        character.level.AddExperience(expAmount);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + (dir * 6f));
    }
}
