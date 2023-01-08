using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public WeaponData weaponData;

    public WeaponStats weaponStats;

    public float timeToAttack = 1f;
    protected float timer;

    public virtual void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0f)
        {
            Attack();
            timer = timeToAttack;
        }
    }

    public virtual void SetData(WeaponData weaponData)
    {
        this.weaponData = weaponData;
        timeToAttack = weaponData.stats.timeToAttack;

        weaponStats = new WeaponStats(weaponData.stats.damage, weaponData.stats.timeToAttack);
    }

    protected virtual void Attack()
    {
        // Do Attack
    }

    public virtual void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition);
    }
}
