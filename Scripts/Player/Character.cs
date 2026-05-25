using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Character : MonoBehaviour
{
    [SerializeField] int currentHealth;
    [SerializeField] GameObject healEffect;
    [SerializeField] Transform[] tearTransforms;
    [SerializeField] GameObject tearEffect;
    [SerializeField] GameObject magnetSynEffectPrefab;
    GameObject magnetSynEffect;
    bool isSynergyMagnetEffectActivated; // 시너지 자석 이펙트가 트리거 되면 그 때부터 계속 발산
    bool isTearEffectActivated;
    Coroutine lavaDrainCoroutine;

    [field: SerializeField] public int MaxHealth { get; set; } = 3000;
    [field: SerializeField] public int Armor { get; set; } = 0;
    [field: SerializeField] public float HpRegenerationRate { get; set; }
    [field: SerializeField] public float HpRegenerationTimer { get; set; }
    [field: SerializeField] public float MagnetSize { get; set; }
    [field: SerializeField] public float Cooldown { get; set; }
    [field: SerializeField] public float MoveSpeed { get; set; } = 6f;
    [field: SerializeField] public float ProjectileAmount { get; set; }
    [field: SerializeField] public float ProjectileSpeed { get; set; }
    [field: SerializeField] public float Area { get; set; }
    [field: SerializeField] public float knockBackChance { get; set; }
    [field: SerializeField] public int DamageBonus { get; set; }
    [field: SerializeField] public float CriticalDamageChance { get; set; }

    [SerializeField] StatusBar hpBar;
    [HideInInspector] public Level level;

    [SerializeField] DataContainer dataContainer;

    [SerializeField] AudioClip hurtSound;

    [SerializeField] ParticleSystem wallCollisionParticle;
    [SerializeField] float wallColParticleDuration; // 벽 충돌 파티클이 보여지는 시간

    bool isHurtSoundPlaying; // hurt sound가 재생 중이면 재생하지 않기 위한 플래그

    [Header("죽음 사운드")]
    [SerializeField] AudioClip deathSound;
    [field: SerializeField] public AudioClip deathVocalSound { get; private set; }

    [Header("부활")]
    [SerializeField] GameObject shockwavePrefab;
    [SerializeField] AudioClip shockWaveSound;

    // public event Action OnDie;
    public UnityEvent OnDie;
    Animator anim;

    DebugCharacter debugCharacter;

    void Awake()
    {
        level = GetComponent<Level>();
    }

    void Start()
    {
        ApplyPersistantUpgrade();
        currentHealth = MaxHealth;
        hpBar.SetStatus(currentHealth, MaxHealth);
        healEffect.SetActive(false);
        tearEffect.SetActive(false);

        wallCollisionParticle = GetComponentInChildren<ParticleSystem>();
        wallCollisionParticle.Stop();
    }

    void Update()
    {
        HpRegenerationTimer += Time.deltaTime * HpRegenerationRate;

        if (HpRegenerationTimer > 1f)
        {
            Heal(1, false);
            HpRegenerationTimer -= 1f;
        }

        MagnetSynergy();
    }

    // 처음 시작할 때 persistant 데이터들을 가져옴.
    void ApplyPersistantUpgrade()
    {
        //int hpUpgradeLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.HP);
        //MaxHealth += MaxHealth / 10 * hpUpgradeLevel;
        //currentHealth = MaxHealth;

        //int damageUpgradeLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.DAMAGE);
        //DamageBonus = 1f + 0.1f * damageUpgradeLevel;

        int ArmorUpgradeLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.Armor);
        Armor += ArmorUpgradeLevel;

        int ProjSpeedUpgradeLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.ProjectileSpeed);
        ProjectileSpeed = ProjSpeedUpgradeLevel;

        int ProJAmountUpgradeLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.ProjectileAmount);
        ProjectileAmount += ProJAmountUpgradeLevel;

        int MagneticArea = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.MagnetRange);
        MagnetSize += 0.25f * MagneticArea * MagnetSize; // 레벨업 당 25% 증가

        int MoveSpeedUpgradeLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.MoveSpeed);
        Logger.Log($"[영구업그레이드] MoveSpeed 레벨: {MoveSpeedUpgradeLevel}, 현재값: {MoveSpeed}");
        MoveSpeed += 0.05f * MoveSpeedUpgradeLevel * MoveSpeed;  // 레벨업 당 5% 증가

        int CooldownUpgradeLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.CoolDown);
        Cooldown -= 0.025f * CooldownUpgradeLevel * Cooldown; // 레벨업 당 2.5% 감소

        int AreaUpgradeLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.Area);
        this.Area += 0.05f * AreaUpgradeLevel * this.Area; // 레벱업 당 5% 증가

        int KnockBackChanceLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.knockBackChance);
        this.knockBackChance += 0.1f * KnockBackChanceLevel * this.knockBackChance; // 레벱업 당 10% 증가

        if (GameManager.instance.startingDataContainer == null)
        {
            Debug.LogError("스타팅 데이터가 없습니다.");
            MaxHealth = 3000;
            DamageBonus = 0;
            return;
        }
        MaxHealth = GameManager.instance.startingDataContainer.GetLeadAttr().Hp;
        if (MaxHealth <= 0)
        {
            Debug.LogError("[Character] MaxHealth = 0! startingDataContainer 초기화 실패.");
            MaxHealth = 3000;
        }
        DamageBonus = GameManager.instance.startingDataContainer.GetLeadAttr().Atk;
        Logger.Log("In Character, Damage Bonus = " + DamageBonus);

        ApplySetBonus();
    }

    void ApplySetBonus()
    {
        SetBonusDefinition bonus = GameManager.instance.startingDataContainer.GetSetBonus();
        if (bonus == null)
        {
            Logger.Log("[Character] 세트 보너스 없음");
            return;
        }

        int grade = GameManager.instance.startingDataContainer.GetSetBonusGrade();

        if (grade < 0 || grade >= 5)
        {
            Logger.LogWarning($"[Character] 세트 등급 범위 오류: {grade}");
            return;
        }

        // ← 적용 전 스탯 저장
        int beforeHp = MaxHealth;
        int beforeAtk = DamageBonus;
        int beforeArmor = Armor;
        float beforeSpeed = MoveSpeed;
        float beforeCooldown = Cooldown;
        float beforeCritical = CriticalDamageChance;
        float beforeHpRegen = HpRegenerationRate;
        float beforeMagnet = MagnetSize;
        float beforeKnockBack = knockBackChance;

        Logger.Log($"[세트 보너스] 세트명: {bonus.setName} / 등급: {grade} / 설명: {bonus.bonusDescription}");
        Logger.Log($"[세트 보너스 적용 전] HP:{beforeHp} ATK:{beforeAtk} Armor:{beforeArmor} Speed:{beforeSpeed:F2} Cooldown:{beforeCooldown:F2} Critical:{beforeCritical:F2} HpRegen:{beforeHpRegen:F2} Magnet:{beforeMagnet:F2} KnockBack:{beforeKnockBack:F2}");

        if (bonus.moveSpeedBonus[grade] != 0)
            MoveSpeed += MoveSpeed * bonus.moveSpeedBonus[grade];

        if (bonus.attackBonus[grade] != 0)
            DamageBonus += (int)(DamageBonus * bonus.attackBonus[grade]);

        // Armor: 고정값으로 변경
        if (bonus.armorBonus[grade] != 0)
            Armor += (int)bonus.armorBonus[grade]; // 퍼센트 아닌 고정값

        if (bonus.maxHpBonus[grade] != 0)
            MaxHealth += (int)(MaxHealth * bonus.maxHpBonus[grade]);

        // Cooldown: 고정값으로 변경
        if (bonus.cooldownBonus[grade] != 0)
            Cooldown -= bonus.cooldownBonus[grade]; // 쿨타임 감소 고정값 (초 단위)

        // CriticalDamageChance: 고정값으로 변경
        if (bonus.criticalChanceBonus[grade] != 0)
            CriticalDamageChance += bonus.criticalChanceBonus[grade]; // 고정값 (0~100 범위)

        if (bonus.hpRegenBonus[grade] != 0)
            HpRegenerationRate += HpRegenerationRate * bonus.hpRegenBonus[grade];

        if (bonus.magnetSizeBonus[grade] != 0)
            MagnetSize += MagnetSize * bonus.magnetSizeBonus[grade];

        if (bonus.knockBackBonus[grade] != 0)
            knockBackChance += knockBackChance * bonus.knockBackBonus[grade];

        // ← 적용 후 스탯 및 변화량 출력
        Logger.Log($"[세트 보너스 적용 후] HP:{MaxHealth}(+{MaxHealth - beforeHp}) ATK:{DamageBonus}(+{DamageBonus - beforeAtk}) Armor:{Armor}(+{Armor - beforeArmor}) Speed:{MoveSpeed:F2}(+{MoveSpeed - beforeSpeed:F2}) Cooldown:{Cooldown:F2}({Cooldown - beforeCooldown:F2}) Critical:{CriticalDamageChance:F2}(+{CriticalDamageChance - beforeCritical:F2}) HpRegen:{HpRegenerationRate:F2}(+{HpRegenerationRate - beforeHpRegen:F2}) Magnet:{MagnetSize:F2}(+{MagnetSize - beforeMagnet:F2}) KnockBack:{knockBackChance:F2}(+{knockBackChance - beforeKnockBack:F2})");
    }

    public void AddDamageBonus(int amount)
    {
        DamageBonus += amount;
        Logger.Log($"[Character] DamageBonus 증가: +{amount} → 총 {DamageBonus}");
    }

    #region TakeDamage
    public void TakeDamage(int damage, EnemyType enemyType, SlimeAttackType attackType = SlimeAttackType.Slime)
    {
        if (GameManager.instance.IsPlayerDead)
            return;
        if (GameManager.instance.IsPlayerInvincible)
            return;

        if (GameManager.instance.fieldItemEffect.IsStopedWithStopwatch()) return; // 스톱워치로 시간이 정지되어 있다면
        if (Time.timeScale == 0) return;

        // 슬로우 모션 상태에서 TakeDamage가 일어나지 않게 하기
        if (BossDieManager.instance.IsBossDead)
            return;

        ApplyArmor(ref damage);

        if (anim == null) anim = GetComponentInChildren<WeaponContainerAnim>().GetComponent<Animator>();

        // 투사체가 아닐 때만 3프레임에 한 번씩 데미지를 받기
        //if (Time.frameCount % 3 != 0 && enemyType != EnemyType.Projectile) return;

        if (attackType == SlimeAttackType.Slime)
        {
            anim.SetTrigger("Hurt");
        }
        else if (attackType == SlimeAttackType.Electricity)
        {
            anim.SetTrigger("ElecHurt");
        }
        else if (attackType == SlimeAttackType.Fire)
        {
            anim.SetTrigger("FireHurt");
        }

        // 피드백
        PlayHurtSound(hurtSound);
        HapticManager.PlayDamage();

        // 임시 테스트용 기본 진동
#if UNITY_ANDROID && !UNITY_EDITOR
Handheld.Vibrate();
#endif

        MessageSystem.instance.PostMessagePlayer(damage.ToString());

        if (isTearEffectActivated == false) StartCoroutine(DoTearParticle());

        currentHealth -= damage;
        if (currentHealth < 0)
        {
            Die();
        }
        else
        {
            hpBar.SetStatus(currentHealth, MaxHealth);
        }

        if (debugCharacter == null) debugCharacter = FindObjectOfType<DebugCharacter>();
    }
    #endregion

    void PlayHurtSound(AudioClip _hurtSound)
    {
        if (isTearEffectActivated) return;
        SoundManager.instance.PlaySoundWith(_hurtSound, .4f, true, .2f);

    }
    IEnumerator DoTearParticle()
    {
        tearEffect.SetActive(true);
        isTearEffectActivated = true;
        yield return new WaitForSeconds(.4f);
        tearEffect.SetActive(false);
        isTearEffectActivated = false;
    }
    void ApplyArmor(ref int damage)
    {
        damage -= Armor;
        if (damage < 0)
        {
            damage = 0;
        }
    }

    public void Heal(int _amount, bool _healingByItem)
    {
        if (currentHealth <= 0)
            return;
        // 자동 힐링은 이펙트 없음. 아이템 힐링은 이펙트
        // 아이템 힐링은 퍼센테이지. 자동 힐링은 정해진 양을 채운다
        if (_healingByItem)
        {
            healEffect.SetActive(true);
            currentHealth += (int)(_amount * MaxHealth / 100);
        }
        else
        {
            currentHealth += _amount;
        }

        if (currentHealth > MaxHealth)
        {
            currentHealth = MaxHealth;
        }
        hpBar.SetStatus(currentHealth, MaxHealth);

    }

    public int GetCurrentHP()
    {
        return currentHealth;
    }

    public void SetTriggerSynergyEffect()
    {
        isSynergyMagnetEffectActivated = true;
    }
    void MagnetSynergy()
    {
        if (isSynergyMagnetEffectActivated == false) return;
        if (Time.frameCount % 30 == 0) return; // 1초에 한 번씩만 자석 효과 발생

        if (magnetSynEffect == null) magnetSynEffect = GameManager.instance.poolManager.GetMisc(magnetSynEffectPrefab);
        magnetSynEffect.transform.position = transform.position;
        magnetSynEffect.transform.SetParent(transform);

        GetComponentInChildren<Magnetic>().MagneticField(MagnetSize + 14f);
    }

    #region Die
    void Die()
    {
        HapticManager.PlayDeath();
        hpBar.gameObject.SetActive(false);
        OnDie?.Invoke();
        StartCoroutine(DieCo());
    }

    IEnumerator DieCo()
    {
        GameManager.instance.pauseManager.PauseGame();

        // ★ 즉시 첫 번째 사운드
        if (deathSound != null)
            SoundManager.instance.Play(deathSound);

        // ★ 0.5초 후 두 번째 사운드 (gameOverVocalSound와 같은 타이밍)
        yield return new WaitForSecondsRealtime(0.5f);
        if (deathVocalSound != null)
            SoundManager.instance.Play(deathVocalSound);

        yield return new WaitForSecondsRealtime(0.3f); // 합계 0.8초

        RevivalPanel revivalPanel = FindObjectOfType<RevivalPanel>();
        if (revivalPanel != null)
        {
            revivalPanel.Show(this);
        }
        else
        {
            Logger.LogWarning("[Character] RevivalPanel을 찾을 수 없습니다. 바로 게임오버.");
            GameManager.instance.pauseManager.UnPauseGame();
            ProcessDeath();
        }
    }
    // RevivalPanel에서 포기/타임아웃 시 호출
    public void ProcessDeath()
    {
        PlayerDataManager playerData = FindObjectOfType<PlayerDataManager>();
        playerData.SaveResourcesBeforeQuitting();
        Logger.Log("In Die, Time Scale = " + Time.timeScale);
        GetComponent<CharacterGameOver>().GameOver();
    }

    // RevivalPanel에서 부활 선택 시 호출
    public void Revive()
    {
        // IsPlayerDead 초기화
        GameManager.instance.IsPlayerDead = false;

        // Player.Die()에서 변경된 mass 복구
        GetComponent<Rigidbody2D>().mass = 100000f;

        currentHealth = MaxHealth;
        hpBar.gameObject.SetActive(true);
        hpBar.SetStatus(currentHealth, MaxHealth);
        GameManager.instance.pauseManager.UnPauseGame();

        // ⭐ 부활 쇼크웨이브 - 데미지 0, 범위 10, 적 레이어만 타겟
        GameObject wave = GameManager.instance.poolManager.GetMisc(shockwavePrefab);
        if (wave != null)
        {
            wave.GetComponent<Shockwave>().Init(0, 10f, LayerMask.GetMask("Enemy"), transform.position);
            SoundManager.instance.Play(shockWaveSound);
        }

        StartCoroutine(InvincibleCo());
        Logger.Log("[Character] 부활 완료. 풀체력 + 무적 3초");
    }
    IEnumerator ReviveCo()
    {
        GameManager.instance.pauseManager.PauseGame();
        yield return new WaitForSeconds(1f);

        // IsPlayerDead 초기화
        GameManager.instance.IsPlayerDead = false;

        // Player.Die()에서 변경된 mass 복구
        GetComponent<Rigidbody2D>().mass = 100000f;

        currentHealth = MaxHealth;
        hpBar.gameObject.SetActive(true);
        hpBar.SetStatus(currentHealth, MaxHealth);
        GameManager.instance.pauseManager.UnPauseGame();

        // ⭐ 부활 쇼크웨이브 - 데미지 0, 범위 10, 적 레이어만 타겟
        GameObject wave = GameManager.instance.poolManager.GetMisc(shockwavePrefab);
        if (wave != null)
        {
            wave.GetComponent<Shockwave>().Init(0, 10f, LayerMask.GetMask("Enemy"), transform.position);
            SoundManager.instance.Play(shockWaveSound);
        }

        StartCoroutine(InvincibleCo());
        Logger.Log("[Character] 부활 완료. 풀체력 + 무적 3초");
    }

    IEnumerator InvincibleCo()
    {
        GameManager.instance.IsPlayerInvincible = true;
        yield return new WaitForSecondsRealtime(3f);
        GameManager.instance.IsPlayerInvincible = false;
        Logger.Log("[Character] 무적 종료");
    }
    #endregion

    public void StartLavaDrain()
    {
        if (lavaDrainCoroutine != null)
            StopCoroutine(lavaDrainCoroutine);

        lavaDrainCoroutine = StartCoroutine(LavaDrainCo());
    }

    public void StopLavaDrain()
    {
        if (lavaDrainCoroutine != null)
        {
            StopCoroutine(lavaDrainCoroutine);
            lavaDrainCoroutine = null;
        }
    }

    IEnumerator LavaDrainCo()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (GameManager.instance.IsPlayerDead) yield break;
            if (GameManager.instance.IsPaused) continue;

            TakeDamage(5, EnemyType.None); // 초당 5 데미지
        }
    }

    #region 디버그
    public void MaxPlayerHealth()
    {
        currentHealth = 10000000;
        MaxHealth = currentHealth;
        hpBar.SetStatus(currentHealth, MaxHealth);
    }
    public void ZeroPlayerHealth()
    {
        currentHealth = 0;
        hpBar.SetStatus(currentHealth, MaxHealth);
        Die();
    }
    #endregion
}
