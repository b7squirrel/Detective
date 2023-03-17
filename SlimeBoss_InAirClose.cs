using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBoss_InAirClose : StateMachineBehaviour
{
    Vector2 target;
    float moveSpeed;
    float timer;
    bool isReadyToLand;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        target = Player.instance.transform.position;
        moveSpeed = animator.GetComponent<EnemyBoss>().moveSpeedInAir;
        animator.GetComponent<EnemyBoss>().SetLayer("InAir");
        timer = .5f;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            animator.SetTrigger("Land");
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<EnemyBoss>().SetLayer("Enemy");
        animator.GetComponent<EnemyBoss>().IsInAir = false;
        animator.GetComponent<EnemyBoss>().transform.GetComponent<CapsuleCollider2D>().isTrigger = false;
    }
}
