using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombWeapon : WeaponBase
{
    [Header("동료 오리용 기본 프로젝타일 (폴백)")]
    [SerializeField] GameObject bomb;
    [SerializeField] float verticalVelocity;

    [Header("설정")]
    [SerializeField] bool isClean;
    [SerializeField] AudioClip shootSFX;

    Vector3[] targetDir = new Vector3[6];
    [SerializeField] GameObject dot;
    Vector3 enemyDir;

    // 런타임에 결정되는 프로젝타일
    GameObject currentBombPrefab;
    float currentVerticalVelocity;

    // Attack에서 매번 new List 하지 않도록 필드로 캐싱
    private List<Vector2> enemyQueryBuffer = new List<Vector2>(1);

    public override void Init(WeaponStats stats, bool isLead)
    {
        base.Init(stats, isLead);
    }

    protected override void OnWeaponDataReady()
    {
        Item equippedItem = GetEssentialEquippedItem();
        if (equippedItem != null && equippedItem.projectilePrefab != null)
        {
            currentBombPrefab = equippedItem.projectilePrefab;
            currentVerticalVelocity = equippedItem.projectileVerticalVelocity;
            Logger.Log($"[BombWeapon] 프로젝타일 사용: {equippedItem.Name} / IsLead: {InitialWeapon}");
        }
        else
        {
            currentBombPrefab = bomb;
            currentVerticalVelocity = verticalVelocity;
            Logger.LogWarning("[BombWeapon] 기본값 사용");
        }
    }

    #region Attack
    protected override void Attack()
    {
        base.Attack();

        // 버퍼 재사용으로 new List 방지
        EnemyFinder.instance.GetEnemies(1, enemyQueryBuffer);
        if (enemyQueryBuffer.Count == 0) return;

        Vector3 axisVec = Vector3.forward;
        enemyDir = (enemyQueryBuffer[0] - (Vector2)transform.position);
        Debug.DrawLine(transform.position, enemyQueryBuffer[0], Color.red);

        float _degree = 360f / weaponStats.numberOfAttacks;
        for (int i = 0; i < weaponStats.numberOfAttacks - 1; i++)
        {
            targetDir[i] = Quaternion.AngleAxis(_degree * (i + 1), axisVec) * enemyDir + transform.position;
            Debug.DrawLine(transform.position, targetDir[i], Color.yellow);
        }

        GenProjectile(enemyQueryBuffer[0]);
        StartCoroutine(AttackCo());
    }

    IEnumerator AttackCo()
    {
        AnimShoot();
        for (int i = 0; i < weaponStats.numberOfAttacks - 1; i++)
        {
            yield return new WaitForSeconds(.2f);
            GenProjectile(targetDir[i]);
        }
    }
    #endregion

    void GenProjectile(Vector3 targetVec)
    {
        if (currentBombPrefab == null)
        {
            Logger.LogError("[BombWeapon] currentBombPrefab이 null입니다!");
            return;
        }

        GameObject bombObject = GameManager.instance.poolManager.GetMisc(currentBombPrefab);
        if (bombObject != null)
        {
            bombObject.transform.position = transform.position;

            ProjectileBase projectileBase = bombObject.GetComponent<ProjectileBase>();
            projectileBase.Damage = GetDamage();
            projectileBase.KnockBackChance = GetKnockBackChance();
            projectileBase.IsCriticalDamageProj = isCriticalDamage;
            projectileBase.WeaponName = weaponData.DisplayName;

            BombProjectile proj = bombObject.GetComponent<BombProjectile>();
            proj.Init(targetVec, weaponStats);

            ProjectileHeight projHeight = bombObject.GetComponent<ProjectileHeight>();
            projHeight.Initialize(currentVerticalVelocity); // 장착 아이템의 velocity 사용
        }

        SoundManager.instance.Play(shootSFX);
    }
}