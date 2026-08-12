using System;
using System.Collections;
using UnityEngine;

public class BossDeadBody : MonoBehaviour, Idamageable
{
    [Header("이펙트")]
    [SerializeField] GameObject teleportEffectPrefab;

    [Header("사운드")]
    [SerializeField] AudioClip crownDropSFX;
    [SerializeField] AudioClip squelchSFX;
    [SerializeField] AudioClip squeackSFX;
    Animator anim;

    [Header("드롭")]
    [SerializeField] GameObject dropPrefab;
    [SerializeField] int dropNums; // 드롭할 기본 개수

    [Header("타격 가능 시간")]
    [SerializeField] float hittableDuration = 3f; // Die 애니메이션 종료 후 텔레포트 전까지 때릴 수 있는 시간

    bool isDamageable; // 아이들 상태로 들어가면 그제서야 데미지를 받고 반응할 수 있다
    public bool FinishBossCam { get; private set; }

    public event Action OnTeleportOutFinished;

    void OnEnable()
    {
        anim = GetComponent<Animator>();
    }

    public void TeleportOutEffect()
    {
        StartCoroutine(TeleportOutEffectCo());
    }

    // Die 애니메이션의 마지막 프레임에 Animation Event로 연결할 함수
    public void OnDieAnimationFinished()
    {
        StartCoroutine(HittableThenTeleportCo());
    }

    // 타격 가능 시간(hittableDuration) 동안 대기한 후 텔레포트 시작
    IEnumerator HittableThenTeleportCo()
    {
        yield return new WaitForSeconds(hittableDuration);
        TeleportOutEffect();
    }

    IEnumerator TeleportOutEffectCo()
    {
        bool teleportAnimDone = false;

        TeleportEffect teleportEffect = GameManager.instance.GetComponent<TeleportEffect>();

        teleportEffect.GenTeleportOutEffect(
            transform.position,
            onVisualHide: HideVisuals,                    // ⭐ 애니메이션 끝나는 즉시 시체를 숨김
            onComplete: () => teleportAnimDone = true      // 파티클 정리까지 끝나면 로직 진행
        );

        while (!teleportAnimDone)
        {
            yield return null;
        }

        isDamageable = false;

        OnTeleportOutFinished?.Invoke();

        gameObject.SetActive(false);
    }

    // 오브젝트를 완전히 비활성화하지 않고, 눈에 보이는 스프라이트와 콜라이더만 숨김/비활성화
    // (SetActive(false)를 하면 진행 중인 코루틴도 멈춰버리기 때문)
    void HideVisuals()
    {
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in sprites)
        {
            sr.enabled = false;
        }

        // ⭐ 추가: 텔레포트 도중에는 더 이상 때릴 수 없도록 콜라이더도 비활성화
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
    }

    //animation events
    public void PlayCrownDropSFX()
    {
        SoundManager.instance.Play(crownDropSFX);
    }
    public void PlayerSquelchSFX()
    {
        SoundManager.instance.Play(squelchSFX);
        SoundManager.instance.Play(squeackSFX);
    }
    public void TriggerPlayerCamera()
    {
        BossDieManager.instance.BossCameraOff();
    }
    public void SetDamageable()
    {
        isDamageable = true;
    }

    public void TakeDamage(int damage, float knockBackChance, float knockBackSpeed, Vector2 target, GameObject hitEffect)
    {
        if (isDamageable)
        {
            anim.SetTrigger("Hit");

            int num = dropNums + UnityEngine.Random.Range(0, 5);
            for (int i = 0; i < num; i++)
            {
                GameObject go = GameManager.instance.poolManager.GetMisc(dropPrefab);
                go.transform.position = transform.position;
            }
        }
    }
}