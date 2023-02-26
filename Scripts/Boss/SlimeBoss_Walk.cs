using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBoss_Walk : StateMachineBehaviour
{
    Transform player;
    Rigidbody2D rb;
    EnemyStats stats;
    bool isKnockBack;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameManager.instance.player.transform;
        rb = animator.GetComponent<Rigidbody2D>();
        stats = animator.GetComponent<EnemyBase>().Stats;
        isKnockBack = animator.GetComponent<EnemyBase>().IsKnockBack;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Flip(animator);
        ApplyMovement(animator);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       
    }

    void Flip(Animator animator)
    {
        if (player.position.x < rb.position.x)
        {
            animator.GetComponent<Transform>().eulerAngles = new Vector3(0, 180f, 0);
        }
        else
        {
            animator.GetComponent<Transform>().eulerAngles = new Vector3(0, 0, 0);
        }
    }
    void ApplyMovement(Animator animator)
    {
        if (isKnockBack)
        {
            rb.velocity = 8f * animator.GetComponent<EnemyBase>().targetDir;
            return;
        }
        Vector2 dirVec = (Vector2)player.position - rb.position;
        Vector2 nextVec = dirVec.normalized * stats.speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + nextVec);
        rb.velocity = Vector2.zero;
    }
}
