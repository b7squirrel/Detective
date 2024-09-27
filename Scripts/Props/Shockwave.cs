using System.Collections;
using UnityEngine;

public class Shockwave : MonoBehaviour
{
    [SerializeField] GameObject hitEffect;
    float range;
    int damage;
    LayerMask target;

    public void Init(int _damage, float _range, LayerMask _layer, Vector2 _pos)
    {
        // 디버깅
        transform.localScale = 2f * _range * Vector2.one;
        Debug.Log("쇼크웨이브 스케일 = " + transform.localScale.x);

        damage = _damage;
        range = _range;
        target = _layer;
        transform.position = _pos;

        StartCoroutine(PushTargetsCo());
    }
    
    // animation events
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
    public void DebuggingTest()
    {
        Debug.Log("Animation");
    }

    IEnumerator  PushTargetsCo()
    {
        yield return null;
        Collider2D[] hitsA = Physics2D.OverlapCircleAll(transform.position, range, target);
        if (hitsA.Length > 0)
        {
            for (int i = 0; i < hitsA.Length; i++)
            {
                hitsA[i].GetComponent<EnemyBase>().TakeDamage(damage, 100f, 4f, transform.position, hitEffect);
                Debug.Log("PUSH A");
            }
        }
        yield return null;
        Collider2D[] hitsB = Physics2D.OverlapCircleAll(transform.position, range, target);
        if (hitsB.Length > 0)
        {
            for (int i = 0; i < hitsB.Length; i++)
            {
                hitsB[i].GetComponent<EnemyBase>().TakeDamage(damage, 100f, 2f, transform.position, hitEffect);
                Debug.Log("PUSH B");
            }
        }
    }
}
