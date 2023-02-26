using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyBoss : EnemyBase, Idamageable
{
    [SerializeField] float timeToAttack;
    [SerializeField] float timer;

    public void Init(EnemyData data)
    {
        this.Stats = new EnemyStats(data.stats);
    }
    public void ShootTimer()
    {
        if(timer < timeToAttack)
        {
            timer += Time.deltaTime;
            return;
        }
        timer = 0f;
        anim.SetTrigger("Shoot");
    }
    public override void TakeDamage(int damage, float knockBackChance)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
        {
            anim.SetTrigger("Hit");
        }
        base.TakeDamage(damage, knockBackChance);
    }
}
