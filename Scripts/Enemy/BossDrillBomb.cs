using System;
using System.Collections;
using UnityEngine;

public class BossDrillBomb : MonoBehaviour
{
    [SerializeField] float waitingTime; // 폭탄이 터지기 전까지의 시간
    [SerializeField] int damage;
    [SerializeField] Transform center; // 폭발의 중심
    float radius; // 폭발 범위
    [SerializeField] LayerMask targetLayer; // 플레이어를 선택하기
    [SerializeField] GameObject damageIndicatorPrefab; // 데미지 인디케이터 프리펩
    [SerializeField] GameObject shockWavePrefab;
    [SerializeField] GameObject explosionEffect;
    Coroutine co;
    Animator anim;
    Action onDie; // 폭탄이 사라질 때 이벤트

    [Header("사운드")]
    [SerializeField] AudioClip[] explosionSounds;

    [Header("디버그")]
    [SerializeField] bool isDubugMode;
    [SerializeField] GameObject debugCircleForCheckingRadius; // 반경 체크를 위한 원

    public void SetDamageRadius(float radius)
    {
        this.radius = radius;
    }
    public void Explode()
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(ExplodeCo());
    }
    IEnumerator ExplodeCo()
    {
        if (anim == null) anim = GetComponent<Animator>();
        anim.SetTrigger("Trigger");
        ShowIndicator();

        yield return new WaitForSeconds(waitingTime);

        PlayExplosionSound(); // 사운드
        CameraShake.instance.Shake(); // 카메라 쉐이크
        Collider2D playerInRange = Physics2D.OverlapCircle(center.position, radius, targetLayer); // 충돌 체크

        if (isDubugMode)
        {
            GameObject circle = Instantiate(debugCircleForCheckingRadius, center.position, Quaternion.identity);
            circle.transform.position = center.position;
            circle.transform.localScale = radius * Vector2.one;
        }

        if (playerInRange != null)
        {
            if (playerInRange.GetComponent<Character>() != null)
            {
                playerInRange.GetComponent<Character>().TakeDamage(damage, EnemyType.Melee);
            }
        }

        GameObject shockWave = GameManager.instance.poolManager.GetMisc(shockWavePrefab);
        shockWave.GetComponent<Shockwave>().Init(0, radius, LayerMask.GetMask("Player"), center.position);
        GameObject explosion = GameManager.instance.poolManager.GetMisc(explosionEffect);
        explosion.GetComponent<ExplosionEffect>().Init(radius, center.position);

        onDie?.Invoke(); // 연결되어 있던 인디케이터를 비활성화
        gameObject.SetActive(false);
    }


    void ShowIndicator()
    {
        GameObject damageIndicator = GameManager.instance.poolManager.GetMisc(damageIndicatorPrefab);
        DamageIndicator indicator = damageIndicator.GetComponent<DamageIndicator>();
        indicator.Init(radius, center.position); // 인디케이터의 반지름이 1 unit으로 되어 있기 때문에 그냥 radius만 곱해줌

        onDie += indicator.DeactivateIndicator; // BossDrillBomb에서 직접 등록
    }

    // 감지 범위 시각화 (Scene 뷰에서 확인 가능)
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    #region 사운드
    void PlayExplosionSound()
    {
        if (explosionSounds == null) return;
        foreach (var item in explosionSounds)
        {
            SoundManager.instance.Play(item);
        }
    }
    #endregion
}
