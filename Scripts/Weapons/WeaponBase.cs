using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public WeaponData weaponData;

    public WeaponStats weaponStats;

    protected float timer;

    public Character Wielder {get; private set;}
    protected bool isSynergyWeaponActivated;

    public Animator anim;
    public Animator animExtra;
    public Transform ShootPoint;
    public Transform EffectPoint;
    public Weapon weaponTools;
    public Weapon weaponToolsExtra;
    public Transform ShootPointExtra;
    public Transform EffectPointExtra;
    protected float angle;
    protected float angleExtra;

    #region Flip
    public bool InitialWeapon{get; set;} // weapon manager에서 설정
    protected float halfHeight, halfWidth;
    protected Vector2 size;
    [SerializeField] protected LayerMask enemy;
    protected WeaponContainerAnim weaponContainerAnim;
    protected Vector2 dir; // 가장 가까운 적으로의 방향
    protected Vector2 dirExtra;
    protected Vector2 direction;
    protected bool flip;
    public bool IsDirectional {get; set;}
    #endregion

    public virtual void Init(WeaponStats stats)
    {
        weaponContainerAnim = GetComponentInParent<WeaponContainerAnim>();
        timer = stats.timeToAttack; // Init이 실행되는 시점에서 weaponStats이 초기화 되지 않아서 stats를 넘겨받아서 timer초기화
        isSynergyWeaponActivated = false;
    }
    protected virtual void Awake()
    {
        halfHeight = Camera.main.orthographicSize;
        halfWidth = Camera.main.aspect * halfHeight;
        size = new Vector2(halfWidth * 2f, halfHeight * 2f);
    }

    protected virtual void Update()
    {
        List<Vector2> closestEnemyPosition = FindTarget(2);
        
        if (closestEnemyPosition[0] == Vector2.zero)
            return;

        dir = GetDirection(closestEnemyPosition[0]);
        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        RotateWeapon();

        dirExtra = GetDirection(closestEnemyPosition[1]);
        angleExtra = Mathf.Atan2(dirExtra.y, dirExtra.x) * Mathf.Rad2Deg;
        RotateExtraWeapon();

        FlipChild();
        FlipWeaponTools();

        timer -= Time.deltaTime;
        if (timer < 0f)
        {
            Attack();
            timer = weaponStats.timeToAttack;
        }
    }

    public virtual void SetData(WeaponData wd)
    {
        this.weaponData = wd;
        weaponStats =
            new WeaponStats(wd.stats.damage, wd.stats.timeToAttack, wd.stats.numberOfAttacks, wd.stats.sizeOfArea, wd.stats.projectileSpeed, wd.stats.knockBackChance);
    }

    protected virtual void Attack()
    {
        // Do Attack
    }

    public int GetDamage()
    {
        // int damage = (int)(weaponData.stats.damage * wielder.DamageBonus);
        int damage = (int)(weaponStats.damage * Wielder.DamageBonus);

        float chance = UnityEngine.Random.Range(0, 100);

        if(chance < Wielder.CriticalDamageChance)
        {
            damage *= 10; // 치명타 데미지는 10배로 일단 가자
        }
        return damage;
    }

    public float GetKnockBackChance()
    {
        float knockBackChance = weaponStats.knockBackChance + Wielder.knockBackChance;
        return knockBackChance;
    }

    public virtual void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition);
    }

    internal void Upgrade(UpgradeData upgradeData)
    {
        weaponStats.Sum(upgradeData.weaponUpgradeStats);
        // 스탯 업그레이드 후
        CheckIfMaxLevel();
    }
    
    void CheckIfMaxLevel()
    {
        // 아이템과는 다르게 알을 먹으면 무기 레벨이 0인 상태로 acquired 되니까 Count와 같음
        if (weaponStats.currentLevel == weaponData.upgrades.Count) 
        {
            Item item = Wielder.GetComponent<PassiveItems>().GetSynergyCouple(weaponData.SynergyWeapon);
                if (item == null)
                {
                    // Debug.Log("시너지 커플 아이템이 없습니다");
                    return;
                }

                // if (item.stats.currentLevel == item.upgrades.Count + 1)
                if (item.stats.currentLevel >= 1) // 아이템을 획득하기만 하면
                {
                    // Debug.Log("wb시너지 웨폰 활성화");
                    Wielder.GetComponent<SynergyManager>().AddSynergyUpgradeToPool(weaponData);
                }
                else
                {
                    // Debug.Log("시너지 커플 아이템이 최고레벨이 아닙니다");
                }
        }
    }

    public void AddOwnerCharacter(Character character)
    {
        Wielder = character;
    }

    protected void AnimShoot()
    {
        if (anim != null)
        {
            anim.SetTrigger("Shoot");
        }
    }
    protected void AnimShootExtra()
    {
        if (animExtra != null)
        {
            animExtra.SetTrigger("Shoot");
        }
    }

    //방향 관련
    protected virtual List<Vector2> FindTarget(int numberOfTargets)
    {
        // 화면 안에서 공격 가능한 개체들 검색
        Collider2D[] hits =
            Physics2D.OverlapBoxAll(transform.position, size, 0f, enemy);
        List<Transform> allEnemies = new List<Transform>();
        foreach (var item in hits)
        {
            Idamageable Idamage = item.GetComponent<Idamageable>();
            if (Idamage != null)
            {
                allEnemies.Add(item.GetComponent<Transform>());
            }
        }

        // 순회하면서 원하는 갯수만큼 공격 가능한 개체들을 수집
        float distanceToclosestEnemy = 20f;
        Transform closestEnemy = null;
        List<Vector2> pickedEnemies = new List<Vector2>();

        for (int i = 0; i < numberOfTargets; i++)
        {
            distanceToclosestEnemy = 20f;
            foreach (Transform item in allEnemies)
            {
                float distanceToEnmey =
                Vector3.Distance(item.position, transform.position);

                if (distanceToEnmey < distanceToclosestEnemy)
                {
                    distanceToclosestEnemy = distanceToEnmey;
                    closestEnemy = item;
                }
            }
            // foreach가 다 돌고 나서 가장 가까운 적이 존재하면
            // 반환할 pickedEnemies에 추가하고, 그 적을 제외하고 다시 순회검색 
            if(closestEnemy != null)
            {
                pickedEnemies.Add(closestEnemy.position);
                allEnemies.Remove(closestEnemy);
            }
            else
            {
                pickedEnemies.Add(Vector2.zero);
            }
        }
        return pickedEnemies;
    }

    protected Vector2 GetDirection(Vector2 closestEnemy)
    {
        direction =
        (closestEnemy - (Vector2)transform.position).normalized;
        return direction;
    }

    protected void RotateWeapon()
    {
        if (weaponTools == null)
            return;
        if (weaponTools.IsDirectional)
            weaponTools.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    protected void RotateExtraWeapon()
    {
        if (weaponToolsExtra == null)
            return;
        if (weaponToolsExtra.IsDirectional)
            weaponToolsExtra.transform.rotation = Quaternion.Euler(0, 0, angleExtra);
    }

    protected void FlipChild()
    {
        if (direction.x < 0)
        {
            flip = true;
        }
        else
        {
            flip = false;
        }

        weaponContainerAnim.Flip(flip);

        //개별 무기 flip
    }

    protected virtual void FlipWeaponTools()
    {
        // flip
        if (weaponTools != null)
        {
            if (dir.x < 0) // 타겟이 왼쪽에 있으면 y축으로 뒤집기
            {
                weaponTools.GetComponentInChildren<SpriteRenderer>().flipY = true;
            }
            else
            {
                weaponTools.GetComponentInChildren<SpriteRenderer>().flipY = false;
            }
        }


        if (weaponToolsExtra != null)
        {
            if (dir.x < 0)
            {
                weaponToolsExtra.GetComponentInChildren<SpriteRenderer>().flipY = true;
            }
            else
            {
                weaponToolsExtra.GetComponentInChildren<SpriteRenderer>().flipY = false;
            }
        }
    }

    public virtual void ActivateSynergyWeapon()
    {
        isSynergyWeaponActivated = true;
        // 개별 무기들에서 각자 구현
    }
    public bool IsSynergyWeaponActivated()
    {
        return isSynergyWeaponActivated;
    }
}
