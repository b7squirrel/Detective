using System;
using UnityEngine;

public class FireBallWeapon : WeaponBase
{
    [SerializeField] GameObject weapon;
    Player player;

    void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    protected override void Attack()
    {
        GameObject fireBall = Instantiate(weapon);
        fireBall.transform.position = transform.position;
        fireBall.GetComponent<FireBallProjectile>().SetDireciotn(player.FacingDir, 0f);
    }
}
