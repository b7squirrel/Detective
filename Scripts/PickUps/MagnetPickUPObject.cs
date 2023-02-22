using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetPickUPObject : Collectable, IPickUpObject
{
    public void OnPickUp(Character character)
    {
        character.GetComponentInChildren<Magnetic>().MagneticField(60f);
    }

    protected override void MoveToPlayer()
    {
        // 자력으로 끌려오지 않는다. 
    }
    public override void OnHitMagnetField(Vector2 direction)
    {
        // 자력에 영향을 받지 않는다
    }
}
