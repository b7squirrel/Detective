using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyBoss : MonoBehaviour
{
    [field : SerializeField] public string BossName{get; private set;}
    public EnemyStats stats;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Init(EnemyData data)
    {
        this.stats = new EnemyStats(data.stats);
    }
}
