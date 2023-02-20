using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetPickUPObject : Collectable, IPickUpObject
{
    public void OnPickUp(Character character)
    {
        character.SizeUpMagnetSize(character);
    }
}
