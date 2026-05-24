using UnityEngine;
using System.Collections;

public class CatFightCloud : MonoBehaviour
{
    public int Damage { get; set; }
    public float KnockBackChance { get; set; }
    public float KnockBackSpeedFactor { get; set; }
    public bool IsCriticalDamageProj { get; set; }
    public string WeaponName { get; set; }
    public float SizeOfArea { get; set; }
    public float Duration { get; set; }

    [Header("Damage Settings")]
    [SerializeField] float damageInterval = 0.3f;

    [Header("Target")]
    [SerializeField] LayerMask target;

    [Header("Effects (Optional)")]
    [SerializeField] GameObject hitEffect;
    [SerializeField] AudioClip hitSFX;

    [Header("Smoke Sprites")]
    [SerializeField] CatFightCloudSprite[] smokeSprites;
    [SerializeField] float maxSmokeStartOffset = 0.3f;
    [SerializeField] Transform smokeTrns;

    [Header("Claw Slashes")]
    [SerializeField] CatClawSlash[] clawSlashes;
    [SerializeField] float maxClawStartOffset = 0.3f;

    [Header("Cat Sounds")]
    [SerializeField] AudioClip[] catMeows;
    [SerializeField] float meowInterval = 0.5f;

    // ✅ 캐싱: Awake에서 한 번만 GetComponent
    HitEffects hitEffects;

    // ✅ NonAlloc용 버퍼
    readonly Collider2D[] damageBuffer = new Collider2D[20];

    Coroutine damageCo;
    Coroutine soundCo;
    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        hitEffects = GetComponent<HitEffects>(); // ✅ 캐싱
    }

    void OnEnable()
    {
        if (anim != null)
            anim.SetTrigger("Start");
    }

    public void Initialize(int damage, float knockBackChance, float knockBackSpeedFactor,
        bool isCriticalDamage, string weaponName, float sizeOfArea, float duration)
    {
        this.Damage = damage;
        this.KnockBackChance = knockBackChance;
        this.KnockBackSpeedFactor = knockBackSpeedFactor;
        this.IsCriticalDamageProj = isCriticalDamage;
        this.WeaponName = weaponName;
        this.SizeOfArea = sizeOfArea;
        this.Duration = duration;

        StartSmokeSprites();
        StartClawSlashes();

        if (damageCo != null) StopCoroutine(damageCo);
        damageCo = StartCoroutine(DamageCo());

        if (soundCo != null) StopCoroutine(soundCo);
        soundCo = StartCoroutine(SoundCo());
    }

    void StartSmokeSprites()
    {
        if (smokeSprites == null || smokeSprites.Length == 0) return;

        for (int i = 0; i < smokeSprites.Length; i++)
        {
            if (smokeSprites[i] != null)
                smokeSprites[i].StartSmoke(Random.Range(0f, maxSmokeStartOffset), SizeOfArea);
        }

        smokeTrns.localScale = (SizeOfArea * 2f) * Vector2.one;
    }

    void StartClawSlashes()
    {
        if (clawSlashes == null || clawSlashes.Length == 0) return;

        for (int i = 0; i < clawSlashes.Length; i++)
        {
            if (clawSlashes[i] != null)
                clawSlashes[i].StartSlash(Random.Range(0f, maxClawStartOffset), SizeOfArea);
        }
    }

    IEnumerator DamageCo()
    {
        float elapsedTime = 0f;
        while (elapsedTime < Duration)
        {
            CastDamage();
            yield return new WaitForSeconds(damageInterval);
            elapsedTime += damageInterval;
        }
        Deactivate();
    }

    IEnumerator SoundCo()
    {
        if (catMeows == null || catMeows.Length == 0) yield break;

        float elapsedTime = 0f;
        while (elapsedTime < Duration)
        {
            AudioClip selectedMeow = catMeows[Random.Range(0, catMeows.Length)];
            if (selectedMeow != null)
                SoundManager.instance.Play(selectedMeow);

            yield return new WaitForSeconds(meowInterval);
            elapsedTime += meowInterval;
        }
    }

    void CastDamage()
    {
        // ✅ NonAlloc으로 GC 방지
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, SizeOfArea, damageBuffer, target);

        for (int i = 0; i < count; i++)
        {
            // ✅ GetComponent 중복 제거
            Idamageable damageable = damageBuffer[i].GetComponent<Idamageable>();
            if (damageable == null) continue;

            if (damageBuffer[i].GetComponent<DestructableObject>() == null)
                PostMessage(Damage, damageBuffer[i].transform.position);

            // ✅ 캐싱된 hitEffects 우선 사용, 없으면 직접 할당된 hitEffect 사용
            GameObject hitEffectObj = hitEffects != null ? hitEffects.hitEffect : hitEffect;

            damageable.TakeDamage(
                Damage, KnockBackChance, KnockBackSpeedFactor,
                transform.position, hitEffectObj);

            if (!string.IsNullOrEmpty(WeaponName))
                DamageTracker.instance.RecordDamage(WeaponName, Damage);
        }

        if (count > 0 && hitSFX != null)
            SoundManager.instance.Play(hitSFX);
    }

    void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition, IsCriticalDamageProj);
    }

    void Deactivate()
    {
        StopSmokeSprites();
        StopClawSlashes();
        gameObject.SetActive(false);
    }

    void StopSmokeSprites()
    {
        if (smokeSprites == null) return;
        for (int i = 0; i < smokeSprites.Length; i++)
            if (smokeSprites[i] != null) smokeSprites[i].StopSmoke();
    }

    void StopClawSlashes()
    {
        if (clawSlashes == null) return;
        for (int i = 0; i < clawSlashes.Length; i++)
            if (clawSlashes[i] != null) clawSlashes[i].StopSlash();
    }

    void OnDisable()
    {
        if (damageCo != null) { StopCoroutine(damageCo); damageCo = null; }
        if (soundCo != null) { StopCoroutine(soundCo); soundCo = null; }
        StopSmokeSprites();
        StopClawSlashes();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, SizeOfArea);
    }
}