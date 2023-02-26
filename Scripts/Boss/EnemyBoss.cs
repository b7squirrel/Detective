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
    protected override void KnockBack()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
            anim.SetTrigger("Hit");
        base.KnockBack();
    }
}
