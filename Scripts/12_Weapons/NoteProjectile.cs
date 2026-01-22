using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteProjectile : MonoBehaviour
{
    GarlicWeapon garlicWeapon;

    // animation evnet
    public void StartAttack()
    {
        if(garlicWeapon == null)
        {
            garlicWeapon = GetComponentInParent<GarlicWeapon>();
        }
    }
}
