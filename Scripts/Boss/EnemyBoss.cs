using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyBoss : EnemyBase, Idamageable
{
    public string BossName {get; private set;}
    public void Init(EnemyData data)
    {
        this.Stats = new EnemyStats(data.stats);
    }
}
