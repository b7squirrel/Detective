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

    public Animator anim;
    public Transform ShootPoint;
    public Transform EffectPoint;
    public Weapon weaponTools;
    protected float angle;

    #region Flip
    public bool InitialWeapon{get; set;} // weapon manager에서 설정
    protected float halfHeight, halfWidth;
    protected Vector2 size;
    [SerializeField] protected LayerMask enemy;
    protected WeaponContainerAnim weaponContainerAnim;
    protected Vector2 dir; // 가장 가까운 적으로의 방향
    protected Vector2 direction;
    protected bool flip;
    public bool IsDirectional {get; set;}
    #endregion


    protected virtual void OnEnable()
    {
        weaponContainerAnim = GetComponentInParent<WeaponContainerAnim>();
        timer = weaponStats.timeToAttack;
    }
    protected virtual void Awake()
    {
        halfHeight = Camera.main.orthographicSize;
        halfWidth = Camera.main.aspect * halfHeight;
        size = new Vector2(halfWidth * 2f, halfHeight * 2f);
    }

    protected virtual void Update()
    {
        Vector2 closestEnemyPosition = FindTarget();
        if (closestEnemyPosition == Vector2.zero)
            return;

        dir = GetDirection(closestEnemyPosition);
        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        RotateWeapon();

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
            Debug.Log(weaponData.Name + " is Max Level");

            Item item = Wielder.GetComponent<PassiveItems>().GetSynergyCouple(weaponData.SynergyWeapon);
                if (item == null)
                {
                    Debug.Log("시너지 커플 아이템이 없습니다");
                    return;
                }

                if (item.stats.currentLevel == item.upgrades.Count + 1)
                {
                    Debug.Log("wb시너지 웨폰 활성화");
                }
                else
                {
                    Debug.Log("시너지 커플 아이템이 최고레벨이 아닙니다");
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

    //방향 관련
    protected virtual Vector2 FindTarget()
    {
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

        float distanceToclosestEnemy = 20f;
        Transform closestEnemy = null;

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

        allEnemies.Clear();

        if (closestEnemy == null)
        {
            return Vector2.zero;
        }

        return closestEnemy.transform.position;
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
        weaponTools.GetComponentInChildren<SpriteRenderer>().flipY = flip;
        
    }
}
