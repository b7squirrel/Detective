using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallProjectile : ProjectileBase
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("MainCamera") || other.gameObject.CompareTag("Wall"))
        {
            gameObject.SetActive(false);
        }
    }
}
