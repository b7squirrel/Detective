using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyBoss : EnemyBase, Idamageable
{
    public string BossName {get; private set;}
    void OnEnable()
    {
        Target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();

        initialMat = sr.material;
        IsKnockBack= false;
    }

    public void Init(EnemyData data)
    {
        this.Stats = new EnemyStats(data.stats);
    }
}
