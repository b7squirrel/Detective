using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// character.MagnetSize 가 0일 때 보석이 pickup되지 않음
// 일단 초기값을 1로 해놓고 넘어갔음
public class Magnetic : MonoBehaviour
{
    Character character;
    Vector2 dir;
    GemManager gemManager;
    float magneticFieldDistance;
    [SerializeField] LayerMask affectedByMagnet;
    private void Awake()
    {
        character = GetComponentInParent<Character>();
        gemManager = FindObjectOfType<GemManager>();
    }

    void Update()
    {
        // if (gemManager.GetGemVisible() == null)
        //     return;

        if (Time.frameCount % 6 != 0)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, character.MagnetSize, affectedByMagnet);
        
        if (hits == null)
            return;

        Debug.Log("Gem numbers Detected " + hits.Length);

        foreach (var item in hits)
        {
            Collectable collectable = item.GetComponent<Collectable>();
            if (collectable != null && collectable.IsHit == false)
            {
                Vector2 dir = item.transform.position - transform.position;
                
                collectable.OnHitMagnetField(dir.normalized);
            }
        }

        // foreach (var item in gemManager.GetGemVisible())
        // {
        //     if (((Vector2)item.position - (Vector2)transform.position).sqrMagnitude > magneticFieldDistance)
        //         continue;

        //     Collectable collectable = item.GetComponent<Collectable>();
        //     if (collectable != null && collectable.IsHit == false)
        //     {
        //         Vector2 dir = item.position - transform.position;
                
        //         collectable.OnHitMagnetField(dir.normalized);
        //     }
        // }
    }

    public void MagneticField(float size)
    {
        StartCoroutine(MagneticFieldCo(size));
    }

    IEnumerator MagneticFieldCo(float size)
    {
        for (int i = 1; i < size; i++)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, i);
            foreach (var item in hits)
            {
                Collectable collectable = item.GetComponent<Collectable>();
                if (collectable != null && collectable.IsHit == false)
                {
                    Vector2 dir = collectable.transform.position - transform.position;
                    collectable.OnHitMagnetField(dir.normalized);
                }
            }

            yield return null;
        }
    }
}
