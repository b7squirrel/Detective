using System;
using UnityEngine;
using UnityEngine.Events;

public class Character : MonoBehaviour
{
    [SerializeField] int maxHealth;
    [SerializeField] int currentHealth;

    [field: SerializeField] public int Armor { get; set; } = 0;
    [field: SerializeField] public float HpRegenerationRate { get; set; }
    [field: SerializeField] public float HpRegenerationTimer { get; set; }
    [field: SerializeField] public float MagnetSize { get; set; }

    [SerializeField] StatusBar hpBar;
    [HideInInspector] public Level level;
    [HideInInspector] public Coins coin;

    [SerializeField] DataContainer dataContainer;

    [field: SerializeField] public float DamageBonus { get; set; }

    // public event Action OnDie;
    public UnityEvent OnDie;

    void Awake()
    {
        level = GetComponent<Level>();
        coin = GetComponent<Coins>();
    }

    void Start()
    {
        ApplyPersistantUpgrade();
        currentHealth = maxHealth;
        hpBar.SetStatus(currentHealth, maxHealth);
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

        maxHealth += maxHealth / 10 * hpUpgradeLevel;
        currentHealth = maxHealth;

        int damageUpgradeLevel = dataContainer.GetUpgradeLevel(PlayerPersistentUpgrades.DAMAGE);

        DamageBonus = 1f + 0.1f * damageUpgradeLevel;
    }

    public void TakeDamage(int damage)
    {
        if (GameManager.instance.IsPlayerDead)
            return;
        ApplyArmor(ref damage);

        currentHealth -= damage;
        if (currentHealth < 0)
        {
            Die();
        }
        else
        {
            hpBar.SetStatus(currentHealth, maxHealth);
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
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        hpBar.SetStatus(currentHealth, maxHealth);
    }

    void Die()
    {
        hpBar.gameObject.SetActive(false);
        OnDie?.Invoke();
        GetComponent<CharacterGameOver>().GameOver();
    }
}
