using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Character : MonoBehaviour
{
    [SerializeField] int currentHealth;

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
    [field: SerializeField] public float DamageBonus { get; set; }

    [SerializeField] StatusBar hpBar;
    [HideInInspector] public Level level;
    [HideInInspector] public Coins coin;

    [SerializeField] DataContainer dataContainer;

    [SerializeField] AudioClip hurtSound;


    // public event Action OnDie;
    public UnityEvent OnDie;
    Animator anim;

    void Awake()
    {
        level = GetComponent<Level>();
        coin = GetComponent<Coins>();

        anim = GetComponent<Animator>();
    }

    void Start()
    {
        ApplyPersistantUpgrade();
        currentHealth = MaxHealth;
        hpBar.SetStatus(currentHealth, MaxHealth);
    }

    void Update()
    {
        HpRegenerationTimer += Time.deltaTime * HpRegenerationRate;

        if (HpRegenerationTimer > 1f)
        {
            Heal(1);
            HpRegenerationTimer -= 1f;
        }
    }

    void ApplyPersistantUpgrade()
    {
        int hpUpgradeLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.HP);
        MaxHealth += MaxHealth / 10 * hpUpgradeLevel;
        currentHealth = MaxHealth;

        int damageUpgradeLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.DAMAGE);
        DamageBonus = 1f + 0.1f * damageUpgradeLevel;

        int ArmorUpgradeLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.Armor);
        Armor += ArmorUpgradeLevel;

        int ProjSpeedUpgradeLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.ProjectileSpeed);
        ProjectileSpeed = ProjSpeedUpgradeLevel;

        int ProJAmountUpgradeLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.ProjectileAmount);
        ProjectileAmount += ProJAmountUpgradeLevel;
        
        int MagneticArea = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.MagnetRange);
        MagnetSize += 0.25f * MagneticArea * MagnetSize; // 레벨업 당 25% 증가

        int MoveSpeedUpgradeLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.MoveSpeed);
        MoveSpeed += 0.05f * MoveSpeedUpgradeLevel * MoveSpeed; // 레벱업 당 5% 증가

        int CooldownUpgradeLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.CoolDown);
        Cooldown -= 0.025f * CooldownUpgradeLevel * Cooldown; // 레벨업 당 2.5% 감소

        int AreaUpgradeLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.Area);
        this.Area += 0.05f * AreaUpgradeLevel * this.Area; // 레벱업 당 5% 증가
        
        int KnockBackChanceLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.knockBackChance);
        this.knockBackChance += 0.1f * KnockBackChanceLevel * this.knockBackChance; // 레벱업 당 10% 증가
    }

    public void TakeDamage(int damage)
    {
        if (GameManager.instance.IsPlayerDead)
            return;
            
        // 슬로우 모션 상태에서 TakeDamage가 일어나지 않게 하기
        if (BossDieManager.instance.IsBossDead) 
            return;
        ApplyArmor(ref damage);

        SoundManager.instance.PlaySingle(hurtSound);
        anim.SetTrigger("Hurt");

        currentHealth -= damage;
        if (currentHealth < 0)
        {
            Die();
        }
        else
        {
            hpBar.SetStatus(currentHealth, MaxHealth);
        }
    }

    void ApplyArmor(ref int damage)
    {
        damage -= Armor;
        if (damage < 0)
        {
            damage = 0;
        }
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0)
            return;
        currentHealth += amount;
        if (currentHealth > MaxHealth)
        {
            currentHealth = MaxHealth;
        }
        hpBar.SetStatus(currentHealth, MaxHealth);
    }

    void Die()
    {
        hpBar.gameObject.SetActive(false);
        OnDie?.Invoke();
        GetComponent<CharacterGameOver>().GameOver();
    }
}
