using System;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] int maxHealth;
    [SerializeField] int currentHealth;

    [field : SerializeField] public int Armor { get; set; } = 0;

    [SerializeField] StatusBar hpBar;
    [HideInInspector] public Level level;
    [HideInInspector] public Coins coin;
 
    public event Action OnDie;

    void Awake()
    {
        level = GetComponent<Level>();
        coin = GetComponent<Coins>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        hpBar.SetStatus(currentHealth, maxHealth);
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
        if (damage < 0 )
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
