using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public WeaponData weaponData;

    public WeaponStats weaponStats;

    // ⭐ 동료 데미지 증가용 배수
    protected float allyDamageMultiplier = 1f;

    // ✨ 데미지 트래커용 무기 이름
    [Header("데미지 트래커")]
    [SerializeField] protected string weaponName;

    protected float timer;
    protected int damage; // Attack이 시작되면 GetDamage()로 얻어냄
    protected float knockback; // Attack이 시작되면 GetKnockBackChance()로 얻어냄
    [SerializeField] protected float knockbackSpeedFactor; // 각 무기의 프리펩에서 직접 입력

    public Character Wielder { get; private set; }
    protected bool isSynergyWeaponActivated;
    protected bool isCriticalDamage; // 크리티컬은 GetDamage에서 결정되고 필요하면 projectileBase에 넘겨진다
    protected bool isLead; // 리드는 공격할 때 크기 변화가 없도록 하기위해

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
    public bool InitialWeapon { get; set; } // weapon manager에서 설정
    [field: SerializeField] public bool NeedParent { get; private set; } // weapon container에서 무기가 생성될 때 어떤 부위에도 parent 시키지 않음 
    protected float halfHeight, halfWidth;
    protected Vector2 size;
    [SerializeField] protected LayerMask enemy;
    protected WeaponContainerAnim weaponContainerAnim;
    Coroutine weaponContainerCo; // 스케일을 할 때는 이전 코루틴을 취소하기 위해
    protected Vector2 dir; // 가장 가까운 적으로의 방향
    protected Vector2 dirExtra;
    protected Vector2 direction;
    protected bool flip;
    public bool IsDirectional { get; set; }

    #endregion

    public virtual void Init(WeaponStats stats, bool isLead)
    {
        weaponContainerAnim = GetComponentInParent<WeaponContainerAnim>();
        timer = stats.timeToAttack; // Init이 실행되는 시점에서 weaponStats이 초기화 되지 않아서 stats를 넘겨받아서 timer초기화
        isSynergyWeaponActivated = false;

        this.isLead = isLead;
    }
    protected virtual void Awake()
    {
        halfHeight = Camera.main.orthographicSize;
        halfWidth = Camera.main.aspect * halfHeight;
        size = new Vector2(halfWidth * 1.4f, halfHeight * 1.4f);
    }

    protected virtual void Update()
    {
        if (GameManager.instance.IsPaused) return;

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
        List<Vector2> closestEnemyPosition = EnemyFinder.instance.GetEnemies(2);

        // null 체크 및 빈 리스트 체크
        if (closestEnemyPosition == null || closestEnemyPosition.Count == 0)
            return;

        // 첫 번째 적 방향 설정
        dir = GetDirection(closestEnemyPosition[0]);
        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 두 번째 적 처리 (안전하게)
        if (closestEnemyPosition.Count > 1)
        {
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
        else
        {
            // 두 번째 적이 없으면 기본값
            angleExtra = angle + 120f;
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

    #region 공격
    protected virtual void GetAttackParameters()
    {
        damage = GetDamage();
        knockback = GetKnockBackChance();
    }

    protected virtual void Attack()
    {
        GetAttackParameters();
        // if (isLead == false) // 리드가 아닐 때만 공격 할 때 커지도록
        // {
        //     if (weaponContainerCo != null) StopCoroutine(weaponContainerCo);
        //     weaponContainerCo = StartCoroutine(SetOriScale(1f));
        // }
        // Do Attack
    }

    IEnumerator SetOriScale(float scaleFactor)
    {
        weaponContainerAnim.transform.localScale = scaleFactor * Vector2.one;
        yield return new WaitForSeconds(.2f);
        weaponContainerAnim.transform.localScale = .8f * Vector2.one;
    }

    /// <summary>
    /// Damage Bonus는 각 무기 damage의 퍼센테이지가 됨
    /// </summary>
    public int GetDamage()
    {
        int damage = (int)new Equation().GetDamage(weaponStats.damage, Wielder.DamageBonus);

        // ⭐ 동료 데미지 배수 적용 (Skill500용)
        if (!InitialWeapon) // 동료들만
        {
            damage = (int)(damage * allyDamageMultiplier);
        }

        float chance = UnityEngine.Random.Range(0, 100);

        if (chance < Wielder.CriticalDamageChance)
        {
            damage = new Equation().GetCriticalDamage(damage);
            isCriticalDamage = true;
        }
        else
        {
            isCriticalDamage = false;
        }
        return damage;
    }

    // ⭐ 동료 데미지 배수 설정 메서드 (Skill500에서 호출)
    public void SetAllyDamageMultiplier(float multiplier)
    {
        allyDamageMultiplier = multiplier;
    }

    // ⭐ 동료 데미지 배수 초기화
    public void ResetAllyDamageMultiplier()
    {
        allyDamageMultiplier = 1f;
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
    #endregion

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
                Logger.Log("시너지 커플 아이템이 없습니다 아이템을 아직 획득하지 못했습니다.");
                return;
            }

            // if (item.stats.currentLevel == item.upgrades.Count + 1)
            if (item.stats.currentLevel >= 1) // 아이템을 획득하기만 하면
            {
                Logger.Log($"[WeaponBase]{weaponData.DisplayName}시너지 웨폰 활성화");
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

    #region 방향 관련
    protected Vector2 GetDirection(Vector2 closestEnemy)
    {
        direction =
        (closestEnemy - (Vector2)transform.position).normalized;
        return direction;
    }

    protected virtual void RotateWeapon()
    {
        if (GameManager.instance.IsPaused) return;

        if (weaponTools == null)
            return;

        if (weaponTools.IsDirectional)
            weaponTools.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    protected void RotateExtraWeapon()
    {
        if (GameManager.instance.IsPaused) return;

        if (weaponToolsExtra == null)
            return;

        if (weaponToolsExtra.IsDirectional)
            weaponToolsExtra.transform.rotation = Quaternion.Euler(0, 0, angleExtra);
    }

    protected virtual void FlipWeaponTools()
    {
        if (GameManager.instance.IsPaused) return;
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
        if (NeedParent == false) // 무기를 페어런트 시켜서 움직이지 않는다면 Flip을 막는다
        {
            transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);
        }
    }
    #endregion

    public virtual void ActivateSynergyWeapon()
    {
        isSynergyWeaponActivated = true;
        // 개별 무기들에서 각자 구현
    }
    public bool IsSynergyWeaponActivated()
    {
        return isSynergyWeaponActivated;
    }

    // 데미지 트래커
    public string GetWeaponName()
    {
        if (weaponData != null)
            return weaponData.DisplayName;

        return gameObject.name; // fallback
    }
}
