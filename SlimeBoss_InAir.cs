using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBoss_InAir : StateMachineBehaviour
{
    Vector2 target;
    float moveSpeed;
    [SerializeField] LayerMask noCollisionLayer;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // collider는 aniamtion event로 껐음
        animator.GetComponent<EnemyBoss>().IsInAir = true;
        target = Player.instance.transform.position;
        moveSpeed = animator.GetComponent<EnemyBoss>().moveSpeedInAir;
        animator.GetComponent<EnemyBoss>().SetLayer("InAir");
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<Transform>().position = Vector2.MoveTowards(animator.GetComponent<Transform>().position, target, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(animator.GetComponent<Transform>().position, target) < 1f)
        {
            animator.SetTrigger("IsClose");
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
