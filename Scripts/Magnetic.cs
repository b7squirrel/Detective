using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnetic : MonoBehaviour
{
    Character character;
    Vector2 dir;

    private void Awake()
    {
        character = GetComponentInParent<Character>();
    }

    void Update()
    {
        if (Time.frameCount % 6 == 0)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, character.MagnetSize);
            foreach (var item in hits)
            {
                Collectable collectable = item.GetComponent<Collectable>();
                if(collectable != null && collectable.IsHit == false)
                {
                    Vector2 dir = collectable.transform.position - transform.position;
                    collectable.OnHitMagnetField(dir.normalized);
                }
            }
        }
    }
}
