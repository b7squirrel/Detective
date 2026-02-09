using System.Collections;
using UnityEngine;

public class LaserPointer : MonoBehaviour
{
    [Header("References")]
    private CatDuckWeapon parentWeapon;

    [Header("Line Renderer Reference")]
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Prefab References")]
    [SerializeField] private GameObject fightCloudPrefab;

    [Header("Laser Visual Effects")]
    [SerializeField] private AudioClip[] laserSounds;

    [Header("Width Settings")]
    [SerializeField] private float baseWidth = 0.18f;
    [SerializeField] private float fireAccentMultiplier = 2.0f;
    [SerializeField] private float accentDuration = 0.12f;

    [Header("Alpha Settings")]
    [Range(0.1f, 1.0f)]
    [SerializeField] private float minAlpha = 0.7f;
    [Range(0.1f, 1.0f)]
    [SerializeField] private float maxAlpha = 1.0f;

    [Header("Cat Settings")]
    private int numberOfCats;
    private int arrivedCats;
    private float damage;
    private float knockback;
    private float knockbackSpeedFactor;
    private bool isCriticalDamage;
    private string weaponDisplayName;
    private float radius;
    private int attackCount;

    private bool isActive = false;
    private Transform laserStartTransform;
    private string weaponName;

    // VFX
    private Coroutine laserVFXCoroutine;
    private float fireAccentTimer = 0f;

    void Awake()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();
    }

    public void Initialize(
        CatDuckWeapon weapon,
        float dmg,
        float kb,
        float kbSpeed,
        bool isCrit,
        string displayName,
        float rad,
        int catCount)
    {
        parentWeapon = weapon;
        laserStartTransform = weapon.transform;
        weaponName = weapon.weaponData.DisplayName;

        damage = dmg;
        knockback = kb;
        knockbackSpeedFactor = kbSpeed;
        isCriticalDamage = isCrit;
        weaponDisplayName = displayName;
        radius = rad;

        numberOfCats = catCount;
        attackCount = catCount;
        arrivedCats = 0;

        isActive = true;

        // LineRenderer 초기화
        lineRenderer.enabled = true;
        lineRenderer.startWidth = baseWidth;
        lineRenderer.endWidth = baseWidth;

        // 시작 / 끝 위치
        lineRenderer.SetPosition(0, laserStartTransform.position);
        lineRenderer.SetPosition(1, transform.position);

        // 발사 강조
        fireAccentTimer = accentDuration;

        // VFX 코루틴
        if (laserVFXCoroutine != null)
            StopCoroutine(laserVFXCoroutine);

        laserVFXCoroutine = StartCoroutine(LaserVFXLoop());

        PlayLaserSounds();
        SpawnCats();
    }

    void LateUpdate()
    {
        if (!isActive || laserStartTransform == null)
            return;

        lineRenderer.SetPosition(0, laserStartTransform.position);
        lineRenderer.SetPosition(1, transform.position);
    }

    public void OnLaserFire()
    {
        fireAccentTimer = accentDuration;
    }

    IEnumerator LaserVFXLoop()
    {
        float time = 0f;

        while (isActive)
        {
            time += Time.deltaTime;

            /* ---------- WIDTH ---------- */
            float pulse =
                Mathf.Sin(time * 8f) * 0.5f + 0.5f;

            float jitter =
                1f + Random.Range(-0.07f, 0.07f);

            float width =
                baseWidth *
                Mathf.Lerp(0.85f, 1.25f, pulse) *
                jitter;

            // 발사 강조
            if (fireAccentTimer > 0f)
            {
                float t = fireAccentTimer / accentDuration;
                width *= Mathf.Lerp(fireAccentMultiplier, 1f, 1f - t);
                fireAccentTimer -= Time.deltaTime;
            }

            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;

            /* ---------- ALPHA ---------- */
            float alphaWave =
                Mathf.Sin(time * 12f) * 0.5f + 0.5f;

            float noise =
                Random.Range(-0.15f, 0.15f);

            float alpha =
                Mathf.Clamp(
                    Mathf.Lerp(minAlpha, maxAlpha, alphaWave) + noise,
                    0f,
                    1f
                );

            Color c = lineRenderer.startColor;
            c.a = alpha;
            lineRenderer.startColor = c;
            lineRenderer.endColor = c;

            yield return null;
        }

        // 종료 시 리셋
        lineRenderer.startWidth = baseWidth;
        lineRenderer.endWidth = baseWidth;

        Color reset = lineRenderer.startColor;
        reset.a = 1f;
        lineRenderer.startColor = reset;
        lineRenderer.endColor = reset;
    }

    private void PlayLaserSounds()
    {
        if (laserSounds == null || laserSounds.Length == 0)
            return;

        foreach (AudioClip clip in laserSounds)
        {
            if (clip != null)
                SoundManager.instance.Play(clip);
        }
    }

    private void SpawnCats()
    {
        if (parentWeapon != null)
        {
            parentWeapon.SpawnCats(transform.position, this, numberOfCats);
        }
    }

    public void OnCatArrived()
    {
        arrivedCats++;

        if (arrivedCats == 1)
            CreateFightCloud();

        if (arrivedCats >= numberOfCats)
            OnAllCatsArrived();
    }

    private void OnAllCatsArrived()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    private void CreateFightCloud()
    {
        GameObject cloudObj =
            GameManager.instance.poolManager.GetMisc(fightCloudPrefab);

        if (cloudObj == null)
            return;

        cloudObj.transform.position = transform.position;

        CatFightCloud cloud = cloudObj.GetComponent<CatFightCloud>();
        if (cloud != null)
        {
            cloud.Initialize(
                attackCount,
                damage,
                radius,
                false,
                weaponName,
                3f,
                3f
            );
        }
    }

    void OnDisable()
    {
        isActive = false;
        arrivedCats = 0;

        if (laserVFXCoroutine != null)
        {
            StopCoroutine(laserVFXCoroutine);
            laserVFXCoroutine = null;
        }
    }
}