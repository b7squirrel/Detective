using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public WeaponData weaponData;

    public WeaponStats weaponStats;

    protected float timer;
    protected int damage; // Attack이 시작되면 GetDamage()로 얻어냄
    protected float knockback; // Attack이 시작되면 GetKnockBackChance()로 얻어냄
    [SerializeField] protected float knockbackSpeedFactor; // 각 무기의 프리펩에서 직접 입력

    public Character Wielder {get; private set;}
    protected bool isSynergyWeaponActivated;
    protected bool isCriticalDamage; // 크리티컬은 GetDamage에서 결정되고 필요하면 projectileBase에 넘겨진다

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
    [field : SerializeField] public bool NeedParent { get; private set; } // weapon container에서 무기가 생성될 때 어떤 부위에도 parent 시키지 않음 
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
        if (Player.instance.IsPauseing) return;

        SetAngle();
        RotateWeapon();
        RotateExtraWeapon();

        FlipWeaponTools();
        LockFlip();

        timer -= Time.deltaTime;
        if (timer < 0f)
        {
            Attack();
            timer = weaponStats.timeToAttack;
        }
    }

    protected virtual void SetAngle()
    {
        List<Vector2> closestEnemyPosition = FindTarget(2);
        dir = GetDirection(closestEnemyPosition[0]);
        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (closestEnemyPosition[1] == Vector2.zero)
        {
            angleExtra = angle + 120f;
        }
        else
        {
            dirExtra = GetDirection(closestEnemyPosition[1]);
            angleExtra = Mathf.Atan2(dirExtra.y, dirExtra.x) * Mathf.Rad2Deg;
        }
    }

    public virtual void SetData(WeaponData wd)
    {
        this.weaponData = wd;
        weaponStats =
            new WeaponStats(wd.stats.damage, 
                            wd.stats.timeToAttack, 
                            wd.stats.numberOfAttacks, 
                            wd.stats.sizeOfArea, 
                            wd.stats.projectileSpeed, 
                            wd.stats.knockBackChance);
    }

    protected virtual void GetAttackParameters()
    {
        damage = GetDamage();
        knockback = GetKnockBackChance();
    }

    protected virtual void Attack()
    {
        GetAttackParameters();
        // Do Attack
    }

    public int GetDamage()
    {
        // int damage = (int)(weaponData.stats.damage * wielder.DamageBonus);
        int damage = (int)(weaponStats.damage + Wielder.DamageBonus);

        float chance = UnityEngine.Random.Range(0, 100);

        if(chance < Wielder.CriticalDamageChance)
        {
            int criticalCoefficient = UnityEngine.Random.Range(5, 9);
            int criticalConstant = UnityEngine.Random.Range(1,100);
            damage = (damage * criticalCoefficient) + criticalConstant; 
            isCriticalDamage = true;
        }
        else
        {
            isCriticalDamage = false;
        }
        return damage;
    }

    public float GetKnockBackChance()
    {
        float knockBackChance = weaponStats.knockBackChance + Wielder.knockBackChance;
        return knockBackChance;
    }

    public bool CheckIsCriticalDamage()
    {
        return isCriticalDamage;
    }

    public virtual void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition, isCriticalDamage);
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

        for (int i = 0; i < hits.Length; i++)
        {
            Idamageable Idamage = hits[i].GetComponent<Idamageable>();
            if (Idamage != null)
            {
                allEnemies.Add(hits[i].GetComponent<Transform>());
            }
        }

        // 순회하면서 원하는 갯수만큼 공격 가능한 개체들을 수집
        float distanceToclosestEnemy = 20f;
        Transform closestEnemy = null;
        List<Vector2> pickedEnemies = new List<Vector2>();

        for (int i = 0; i < numberOfTargets; i++)
        {
            distanceToclosestEnemy = 20f;
            for (int y = 0; y < allEnemies.Count; y++)
            {
                float distanceToEnmey =
                Vector3.Distance(allEnemies[y].position, transform.position);

                if (distanceToEnmey < distanceToclosestEnemy)
                {
                    distanceToclosestEnemy = distanceToEnmey;
                    closestEnemy = allEnemies[y];
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

    protected virtual void RotateWeapon()
    {
        if (Player.instance.IsPauseing) return;

        if (weaponTools == null)
            return;

        if (weaponTools.IsDirectional)
            weaponTools.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    protected void RotateExtraWeapon()
    {
        if (Player.instance.IsPauseing) return;

        if (weaponToolsExtra == null)
            return;

        if (weaponToolsExtra.IsDirectional)
            weaponToolsExtra.transform.rotation = Quaternion.Euler(0, 0, angleExtra);
    }

    protected virtual void FlipWeaponTools()
    {
        if (Player.instance.IsPauseing) return;
        if (weaponTools == null) return;
        //if (weaponToolsExtra == null) return;

        if (weaponTools.IsDirectional == false) return;

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
                weaponToolsExtra.GetComponentInChildren<SpriteRenderer>().flipY = false;
            }
            else
            {
                weaponToolsExtra.GetComponentInChildren<SpriteRenderer>().flipY = true;
            }
        }
    }

    protected virtual void LockFlip()
    {
        if(NeedParent == false) // 무기를 페어런트 시켜서 움직이지 않는다면 Flip을 막는다
        {
            transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);
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
