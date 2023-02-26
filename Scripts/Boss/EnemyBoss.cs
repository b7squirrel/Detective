using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyBoss : EnemyBase, Idamageable
{
    public void Init(EnemyData data)
    {
        this.Stats = new EnemyStats(data.stats);
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
