using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBoss_Shoot : StateMachineBehaviour
{
    Rigidbody2D rb;
    bool isKnockBack;
    EnemyBase enemyBase;
    EnemyBoss enemyBoss;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb = animator.GetComponent<Rigidbody2D>();
        enemyBase = animator.GetComponent<EnemyBase>();
        enemyBoss = animator.GetComponent<EnemyBoss>();
        isKnockBack = enemyBase.IsKnockBack;

        enemyBoss.ShootProjectile();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb.velocity = Vector2.zero;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       enemyBoss.StopShooting();
    }
}
