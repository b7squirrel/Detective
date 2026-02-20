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
    protected int damage;
    protected float knockback;
    [SerializeField] protected float knockbackSpeedFactor;

    public Character Wielder { get; private set; }
    protected bool isSynergyWeaponActivated;
    protected bool isCriticalDamage;
    protected bool isLead;

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
    public bool InitialWeapon { get; set; }
    [field: SerializeField] public bool NeedParent { get; private set; }
    protected float halfHeight, halfWidth;
    protected Vector2 size;
    [SerializeField] protected LayerMask enemy;
    protected WeaponContainerAnim weaponContainerAnim;
    Coroutine weaponContainerCo;
    protected Vector2 dir;
    protected Vector2 dirExtra;
    protected Vector2 direction;
    protected bool flip;
    public bool IsDirectional { get; set; }
    #endregion

    public virtual void Init(WeaponStats stats, bool isLead)
    {
        weaponContainerAnim = GetComponentInParent<WeaponContainerAnim>();
        timer = stats.timeToAttack;
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

        if (closestEnemyPosition == null || closestEnemyPosition.Count == 0)
            return;

        dir = GetDirection(closestEnemyPosition[0]);
        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

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

        // ⭐ weaponData 할당 완료 후 호출
        OnWeaponDataReady();
    }

    // ⭐ 각 무기에서 override해서 프로젝타일 결정
    protected virtual void OnWeaponDataReady() { }

    #region 공격
    protected virtual void GetAttackParameters()
    {
        damage = GetDamage();
        knockback = GetKnockBackChance();
    }

    protected virtual void Attack()
    {
        GetAttackParameters();
    }

    IEnumerator SetOriScale(float scaleFactor)
    {
        weaponContainerAnim.transform.localScale = scaleFactor * Vector2.one;
        yield return new WaitForSeconds(.2f);
        weaponContainerAnim.transform.localScale = .8f * Vector2.one;
    }

    public int GetDamage()
    {
        int damage = (int)new Equation().GetDamage(weaponStats.damage, Wielder.DamageBonus);

        if (!InitialWeapon)
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

    public void SetAllyDamageMultiplier(float multiplier)
    {
        allyDamageMultiplier = multiplier;
    }

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
        CheckIfMaxLevel();
    }

    void CheckIfMaxLevel()
    {
        if (weaponStats.currentLevel == weaponData.upgrades.Count)
        {
            Item item = Wielder.GetComponent<PassiveItems>().GetSynergyCouple(weaponData.SynergyWeapon);
            if (item == null)
            {
                Logger.Log("시너지 커플 아이템이 없습니다 아이템을 아직 획득하지 못했습니다.");
                return;
            }

            if (item.stats.currentLevel >= 1)
            {
                Logger.Log($"[WeaponBase]{weaponData.DisplayName}시너지 웨폰 활성화");
                Wielder.GetComponent<SynergyManager>().AddSynergyUpgradeToPool(weaponData);
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
        direction = (closestEnemy - (Vector2)transform.position).normalized;
        return direction;
    }

    protected virtual void RotateWeapon()
    {
        if (GameManager.instance.IsPaused) return;
        if (weaponTools == null) return;
        if (weaponTools.IsDirectional)
            weaponTools.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    protected void RotateExtraWeapon()
    {
        if (GameManager.instance.IsPaused) return;
        if (weaponToolsExtra == null) return;
        if (weaponToolsExtra.IsDirectional)
            weaponToolsExtra.transform.rotation = Quaternion.Euler(0, 0, angleExtra);
    }

    protected virtual void FlipWeaponTools()
    {
        if (GameManager.instance.IsPaused) return;
        if (weaponTools == null) return;
        if (weaponTools.IsDirectional == false) return;

        if (weaponTools != null)
        {
            if (dir.x < 0)
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
        if (NeedParent == false)
        {
            transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);
        }
    }
    #endregion

    public virtual void ActivateSynergyWeapon()
    {
        isSynergyWeaponActivated = true;
    }

    public bool IsSynergyWeaponActivated()
    {
        return isSynergyWeaponActivated;
    }

    public string GetWeaponName()
    {
        if (weaponData != null)
            return weaponData.DisplayName;
        return gameObject.name;
    }

    // ⭐⭐⭐ 새로 추가: 장착 아이템 조회 메서드 ⭐⭐⭐

    /// <summary>
    /// 리드 오리의 장착된 아이템을 가져옵니다.
    /// </summary>
    /// <param name="slotType">장비 슬롯 타입 (Head=0, Chest=1, Face=2, Hand=3)</param>
    /// <returns>장착된 Item, 없으면 null</returns>
    protected Item GetEquippedItem(EquipmentType slotType)
    {
        // 리드 오리가 아니면 null 반환
        if (!InitialWeapon)
        {
            return null;
        }

        // StartingDataContainer에서 장착 아이템 가져오기
        StartingDataContainer container = GameManager.instance.startingDataContainer;
        if (container == null)
        {
            Logger.LogWarning("[WeaponBase] StartingDataContainer를 찾을 수 없습니다.");
            return null;
        }

        List<Item> equippedItems = container.GetItemDatas();
        int slotIndex = (int)slotType;

        if (slotIndex < 0 || slotIndex >= equippedItems.Count)
        {
            // Logger.LogWarning($"[WeaponBase] 잘못된 슬롯 인덱스: {slotIndex}");
            return null;
        }

        return equippedItems[slotIndex];
    }

    /// <summary>
    /// 필수 장비 슬롯의 아이템을 가져옵니다.
    /// </summary>
    /// <returns>필수 장비 Item, 없으면 null</returns>
    protected Item GetEssentialEquippedItem()
    {
        StartingDataContainer container = GameManager.instance.startingDataContainer;
        if (container == null)
        {
            Logger.LogWarning("[WeaponBase] StartingDataContainer를 찾을 수 없습니다.");
            return null;
        }

        if (InitialWeapon) // 리드 오리 - 기존 방식 유지
        {
            int essentialIndex = container.GetEssectialIndex();
            if (essentialIndex < 0) return null;

            List<Item> equippedItems = container.GetItemDatas();
            if (essentialIndex >= equippedItems.Count) return null;
            return equippedItems[essentialIndex];
        }
        else // 동료 오리 - weaponData.equipmentType을 인덱스로 사용
        {
            if (weaponData?.defaultItems == null) return null;

            int allyIndex = (int)weaponData.equipmentType;
            if (allyIndex < 0 || allyIndex >= weaponData.defaultItems.Length) return null;

            return weaponData.defaultItems[allyIndex];
        }
    }
}