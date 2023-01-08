using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemPickUpObject : MonoBehaviour, IPickUpObject
{
    [SerializeField] private int expAmount;
    public void OnPickUp(Character character)
    {
        character.level.AddExperience(expAmount);
    }

}
