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
    private void Awake()
    {
        character = GetComponentInParent<Character>();
        gemManager = FindObjectOfType<GemManager>();
    }

    void Update()
    {
        if (gemManager.GetGemVisible() == null)
            return;

        if (Time.frameCount % 6 != 0)
            return;

        if (character.MagnetSize == 0)
        {
            magneticFieldDistance = 1;
        }
        else
        {
            magneticFieldDistance = Mathf.Pow(character.MagnetSize, 2);
        }

        foreach (var item in gemManager.GetGemVisible())
        {
            if (((Vector2)item.position - (Vector2)transform.position).sqrMagnitude > magneticFieldDistance)
                continue;

            Collectable collectable = item.GetComponent<Collectable>();
            if (collectable != null && collectable.IsHit == false)
            {
                Vector2 dir = item.position - transform.position;
                
                collectable.OnHitMagnetField(dir.normalized);
            }
        }
    }

    public void MagneticField(float size)
    {
        StartCoroutine(MagneticFieldCo(size));
    }

    IEnumerator MagneticFieldCo(float size)
    {
        magneticFieldDistance = Mathf.Pow(character.MagnetSize, 2);

        for (int i = 1; i < size; i++)
        {
            if (gemManager.GetGemVisible() == null)
                break;

            foreach (var item in gemManager.GetGemVisible())
            {
                if ((item.position - transform.position).sqrMagnitude > Mathf.Pow(i, 2))
                    continue;

                Collectable collectable = item.GetComponent<Collectable>();
                if (collectable != null && collectable.IsHit == false)
                {
                    Vector2 dir = item.position - transform.position;
                    collectable.OnHitMagnetField(dir.normalized);
                }
            }

            yield return null;
        }
    }
}
