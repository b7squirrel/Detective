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
                if (target == LayerMask.GetMask("Enemy"))
                {
                    EnemyBase enemy = hitsA[i].GetComponent<EnemyBase>();
                    if (enemy != null) enemy.TakeDamage(damage, 100f, 4f, transform.position, hitEffect);
                }
                if (target == LayerMask.GetMask("Player"))
                {
                    Character ch = hitsA[i].GetComponent<Character>();
                    if (ch != null) ch.TakeDamage(damage, EnemyType.Ranged); // Melee는 3프레임에 한 번 데미지를 입게 되므로
                }
            }
        }
    }
}
