using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBoss_ShootAntic : StateMachineBehaviour
{
    Rigidbody2D rb;
    bool isKnockBack;
    EnemyBase enemyBase;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb = animator.GetComponent<Rigidbody2D>();
        enemyBase = animator.GetComponent<EnemyBase>();
        isKnockBack = enemyBase.IsKnockBack;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb.velocity = Vector2.zero;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       
    }
}
