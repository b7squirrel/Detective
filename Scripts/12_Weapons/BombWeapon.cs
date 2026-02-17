using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombWeapon : WeaponBase
{
    [Header("동료 오리용 기본 프로젝타일 (폴백)")]
    [SerializeField] GameObject bomb; // ⭐ 동료 오리는 여전히 이것을 사용
    [SerializeField] float verticalVelocity;
    
    [Header("설정")]
    [SerializeField] bool isClean;
    [SerializeField] AudioClip shootSFX;
    
    Vector3[] targetDir = new Vector3[6];
    [SerializeField] GameObject dot;
    Vector3 enemyDir;

    // ⭐ 런타임에 결정되는 프로젝타일
    GameObject currentBombPrefab;
    float currentVerticalVelocity;

    public override void Init(WeaponStats stats, bool isLead)
    {
        base.Init(stats, isLead);

        // ⭐ 프로젝타일 결정
        if (InitialWeapon) // 리드 오리
        {
            Item equippedItem = GetEssentialEquippedItem();
            
            if (equippedItem != null && equippedItem.projectilePrefab != null)
            {
                // 장착된 아이템의 프로젝타일 사용
                currentBombPrefab = equippedItem.projectilePrefab;
                currentVerticalVelocity = equippedItem.projectileVerticalVelocity;
                
                Logger.Log($"[BombWeapon] 리드 오리 - 장착 아이템 프로젝타일 사용: {equippedItem.Name}");
            }
            else
            {
                // 폴백: 인스펙터의 기본 프로젝타일 사용
                currentBombPrefab = bomb;
                currentVerticalVelocity = verticalVelocity;
                
                Logger.LogWarning("[BombWeapon] 리드 오리 - 장착된 프로젝타일이 없어서 기본값 사용");
            }
        }
        else // 동료 오리
        {
            // 인스펙터의 프로젝타일 사용
            currentBombPrefab = bomb;
            currentVerticalVelocity = verticalVelocity;
            
            Logger.Log("[BombWeapon] 동료 오리 - 기본 프로젝타일 사용");
        }
    }

    #region Attack
    protected override void Attack()
    {
        List<Vector2> _enemyPos = EnemyFinder.instance.GetEnemies(1);
        if (_enemyPos.Count == 0) { return; }
        
        Vector3 axisVec = Vector3.forward;
        enemyDir = (_enemyPos[0] - (Vector2)transform.position);
        Debug.DrawLine(transform.position, _enemyPos[0], Color.red);
        
        float _degree = 360 / weaponStats.numberOfAttacks;
        for (int i = 0; i < weaponStats.numberOfAttacks - 1; i++)
        {
            targetDir[i] = Quaternion.AngleAxis((float)(_degree * (i + 1)), axisVec) * enemyDir + transform.position;
            Debug.DrawLine(transform.position, targetDir[i], Color.yellow);
        }
        
        GenProjectile(_enemyPos[0]);
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
        // ⭐ currentBombPrefab 사용 (리드 오리면 장착 아이템, 동료면 기본값)
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
            
            // ✨ 투사체에 무기 이름 전달
            projectileBase.WeaponName = weaponData.DisplayName;
            
            BombProjectile proj = bombObject.GetComponent<BombProjectile>();
            proj.Init(targetVec, weaponStats);
            
            ProjectileHeight projHeight = bombObject.GetComponent<ProjectileHeight>();
            projHeight.Initialize(currentVerticalVelocity); // ⭐ 장착 아이템의 velocity 사용
        }
        
        SoundManager.instance.Play(shootSFX);
    }
}