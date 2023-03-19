using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombWeapon : WeaponBase
{
    [SerializeField] GameObject bomb;
    [SerializeField] float duration = .4f;
    List<Vector2> targets; //폭탄을 던질 지점들
    [SerializeField] bool isClean;


    [SerializeField] GameObject testCircle;

    #region Attack
    protected override void Attack()
    {
        FindLandingPositions(); // number Of Attacks 만큼 target을 잡음

        if (isClean) // 화면 상에 적이 없으면
        {
            isClean = false;
            return;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            GameObject bombObject = Instantiate(bomb, transform.position, Quaternion.identity);
            bombObject.GetComponent<BombProjectile>().SetTargetDirection(targets[i]);
        }
        targets.Clear();
    }
    #endregion

    #region Find Landing Position
    void FindLandingPositions()
    {
        if (targets == null)
            targets = new List<Vector2>();

        Vector2 center = GameManager.instance.player.transform.position;

        Collider2D[] enemies =
                Physics2D.OverlapAreaAll(center - new Vector2(halfWidth * .8f, halfHeight * .8f),
                                            center + new Vector2(halfWidth * .8f, halfHeight * .8f), enemy);

        if (enemies.Length == 0)
        {
            isClean = true;
            return;
        }

        List<Vector2> candidates = new List<Vector2>();

        for (int i = 0; i < enemies.Length; i++)
        {
            candidates.Add((Vector2)enemies[i].transform.position);
        }

        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            int index = Random.Range(0, candidates.Count);
            Vector2 pick = candidates[index];
            targets.Add(pick);
        }
    }
    #endregion
}
