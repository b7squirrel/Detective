using System.Collections;
using UnityEngine;

public class PartyProjectile : MonoBehaviour
{
    public int Damage { get; set; }
    public float KnockBackChance { get; set; }
    public float KnockBackSpeedFactor { get; set; }
    public bool IsCriticalDamageProj { get; set; }
    public string WeaponName { get; set; }
    public float SizeOfArea { get; set; }

    [Header("Ground Behavior")]
    [SerializeField] float groundDuration = 3f;
    [SerializeField] float damageInterval = 0.5f;

    [Header("Effects")]
    [SerializeField] GameObject hitEffect;
    [SerializeField] GameObject starEffect;
    [SerializeField] AudioClip hitSFX;
    [SerializeField] GameObject groundLoopEffect;

    [Header("Target")]
    [SerializeField] LayerMask target;

    // ✅ 캐싱: Awake에서 한 번만 GetComponent
    ShadowHeight shadowHeight;
    HitEffects hitEffects;
    TrailRenderer trailRenderer;
    Animator anim;

    bool isGrounded = false;
    Coroutine groundDamageCo;

    // ✅ NonAlloc용 버퍼
    readonly Collider2D[] groundDamageBuffer = new Collider2D[20];

    void Awake()
    {
        shadowHeight = GetComponent<ShadowHeight>();
        hitEffects = GetComponent<HitEffects>();           // ✅ 캐싱
        trailRenderer = GetComponentInChildren<TrailRenderer>(); // ✅ 캐싱
        anim = GetComponentInChildren<Animator>();         // ✅ 캐싱

        if (shadowHeight == null)
            Debug.LogError("PartyProjectile: ShadowHeight component not found!");
    }

    void OnEnable()
    {
        isGrounded = false;

        // ✅ 캐싱된 trailRenderer 사용
        if (trailRenderer != null)
            trailRenderer.enabled = true;
    }

    void FixedUpdate()
    {
        if (shadowHeight == null) return;

        if (!isGrounded && shadowHeight.IsDone)
            OnFullyGrounded();
    }

    void OnFullyGrounded()
    {
        isGrounded = true;

        // ✅ 캐싱된 anim 사용
        if (anim != null)
            anim.SetTrigger("Landed");

        if (groundDamageCo != null)
            StopCoroutine(groundDamageCo);
        groundDamageCo = StartCoroutine(GroundDamageCo());
    }

    IEnumerator GroundDamageCo()
    {
        float elapsedTime = 0f;

        GameObject loopEffect = null;
        if (groundLoopEffect != null)
        {
            loopEffect = GameManager.instance.poolManager.GetMisc(groundLoopEffect);
            if (loopEffect != null)
            {
                loopEffect.transform.position = transform.position;
                loopEffect.transform.localScale = Vector2.one * SizeOfArea;
            }
        }

        while (elapsedTime < groundDuration)
        {
            CastGroundDamage();
            yield return new WaitForSeconds(damageInterval);
            elapsedTime += damageInterval;
        }

        if (loopEffect != null)
            loopEffect.SetActive(false);

        Deactivate();
    }

    void CastGroundDamage()
    {
        // ✅ NonAlloc으로 GC 방지
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, 2f, groundDamageBuffer, target);

        // ✅ 캐싱된 hitEffects 사용, 없으면 직접 할당된 hitEffect 사용
        GameObject hitEffectObj = hitEffects != null ? hitEffects.hitEffect : hitEffect;

        for (int i = 0; i < count; i++)
        {
            // ✅ GetComponent 중복 호출 제거
            Idamageable damageable = groundDamageBuffer[i].GetComponent<Idamageable>();
            if (damageable == null) continue;

            if (groundDamageBuffer[i].GetComponent<DestructableObject>() == null)
                PostMessage(Damage, groundDamageBuffer[i].transform.position);

            damageable.TakeDamage(
                Damage, KnockBackChance, KnockBackSpeedFactor,
                transform.position, hitEffectObj);

            if (!string.IsNullOrEmpty(WeaponName))
                DamageTracker.instance.RecordDamage(WeaponName, Damage);
        }
    }

    void Deactivate()
    {
        // ✅ 캐싱된 trailRenderer 사용
        if (trailRenderer != null)
            trailRenderer.enabled = false;

        gameObject.SetActive(false);
    }

    public void Explode()
    {
        GenerateHitEffect();
        SoundManager.instance.Play(hitSFX);
        CastGroundDamage();

        if (trailRenderer != null)
            trailRenderer.enabled = false;

        gameObject.SetActive(false);
    }

    void GenerateHitEffect()
    {
        if (hitEffect != null)
        {
            GameObject smoke = GameManager.instance.poolManager.GetMisc(hitEffect);
            smoke.transform.position = transform.position;
            smoke.transform.localScale = Vector2.one * SizeOfArea;
        }

        if (starEffect != null)
        {
            GameObject stars = GameManager.instance.poolManager.GetMisc(starEffect);
            stars.transform.position = transform.position;
            stars.transform.localScale = Vector2.one * SizeOfArea;
        }
    }

    void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition, IsCriticalDamageProj);
    }

    void OnDisable()
    {
        if (groundDamageCo != null)
        {
            StopCoroutine(groundDamageCo);
            groundDamageCo = null;
        }
        isGrounded = false;
    }
}