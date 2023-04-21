using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// duration, damage 주기를 변수로 설정하기
public class LaserWeapon : WeaponBase
{
    [SerializeField] GameObject test;
    [SerializeField] GameObject laserBox;
    LineRenderer lnRen = new LineRenderer();
    [SerializeField] GameObject laserPrefab;
    [SerializeField] GameObject hitEffect;
    [SerializeField] AudioClip shootSFX;
    [SerializeField] GameObject shootEffectPrefab;
    GameObject shootEffect;
    [SerializeField] GameObject dot;
    [SerializeField] Material laserMat;
    [SerializeField] float laserLifeTIme;
    [SerializeField] float damageFrequency = .15f;
    float duration;
    Vector2 targetDir; // 가장 가까운 적 dir을 고정시키기 위한 변수
    Vector2 targetPos; // 레이져가 부딪친 지점
    float initialWidth = .4f;
    Coroutine shootCoroutine;
    [SerializeField] bool isShooting;
    [SerializeField] bool isWaitingToShoot;
    float laserRotationSpeedRate; // 레이져 회전 속도
    int numberOfLaser; // 레이져 갯수
    float laserAngle; // 레이져 갯수에 따른 각도
    GameObject laserObject;
    Color lrColor;

    LaserTexture laserTexture;

    protected override void OnEnable()
    {
        base.OnEnable();
        test = transform.parent.gameObject;
        laserObject = Instantiate(laserPrefab, transform);
        laserObject.transform.position = transform.position;

        lnRen = laserObject.GetComponent<LineRenderer>();
        lnRen.material = laserMat;
        lnRen.enabled = false;
        lnRen.startWidth = initialWidth;
        lnRen.endWidth = initialWidth;
        lnRen.sortingLayerName = "FloatingOver";
        // lrColor = new Color(1, .74f, 0, 1);
        lrColor = new Color(1, 1, 1, 1);

        lnRen.startColor = lrColor;
        lnRen.endColor = lrColor;

        ShootPoint = transform;
        shootEffect = Instantiate(shootEffectPrefab, transform);
        shootEffect.transform.position = transform.position;
        shootEffect.gameObject.SetActive(false);
        duration = laserLifeTIme;

        timer = weaponStats.timeToAttack;

        laserTexture = GetComponent<LaserTexture>();
    }
    protected override void Update()
    {
        FlipChild();

        if (isShooting)
        {
            if (duration > 0)
            {
                duration -= Time.deltaTime;
            }
            else
            {
                isShooting = false;
                lnRen.enabled = false;
                duration = laserLifeTIme;
                shootEffect.gameObject.SetActive(false);
            }
        }

        if (isShooting == false)
        {
            if (shootCoroutine != null)
            {
                StopCoroutine(shootCoroutine);
            }
            if (timer > 0) // 쿨타임은 레이져를 안쏘고 있을 때만 돌아감
            {
                timer -= Time.deltaTime;
            }
            else
            {
                timer = weaponStats.timeToAttack;

                isShooting = true;

                targetPos = FindTarget();
                if(targetPos == Vector2.zero) return;
                targetDir = (targetPos - (Vector2)transform.position).normalized;

                lnRen.startWidth = initialWidth;
                lnRen.endWidth = initialWidth;
                lnRen.startColor = lrColor;
                lnRen.endColor = lrColor;

                laserTexture.animationStep = 0;
                shootCoroutine = StartCoroutine(ShootLaser());
            }
        }
    }

    IEnumerator ShootLaser()
    {
        SoundManager.instance.Play(shootSFX);
        int count = 1;
        float remainder = count % 3;

        while (isShooting)
        {
            lnRen.enabled = true;
            shootEffect.gameObject.SetActive(true);
            RaycastHit2D rayHit = Physics2D.Raycast(transform.position, targetDir, 120f, enemy);
            Debug.DrawLine(transform.position, targetDir * 120f, Color.red);
            lnRen.SetPosition(0, transform.position);
            lnRen.SetPosition(1, transform.position + (120f * (Vector3)targetDir));

            if(count % 3 > 0) // 3번에 한 번만 데미지 입히기
            {
                CastDamageToTarget(rayHit.point);
            }

            // lnRen.startWidth -= (Time.deltaTime * 2f);
            // if (lnRen.startWidth < .1f)
            // {
            //     lnRen.startWidth = .1f;
            // }
            // lnRen.endWidth = lnRen.startWidth;
            yield return null;
        }
    }

    void CastDamageToTarget(Vector2 targetPoint)
    {
        Collider2D[] hit = Physics2D.OverlapCircleAll(targetPoint, .1f);
        if (hit == null) return;
        foreach (var item in hit)
        {
            int damage = GetDamage();
            float knockBackChance = weaponStats.knockBackChance;
            Transform enmey = item.GetComponent<Transform>();
            if (enmey.GetComponent<Idamageable>() != null)
            {
                PostMessage(damage, enmey.transform.position);
                enmey.GetComponent<Idamageable>().TakeDamage(damage, knockBackChance, transform.position);
                break;
            }
        }
    }
}
