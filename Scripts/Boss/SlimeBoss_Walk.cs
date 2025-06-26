using UnityEngine;

public class SlimeBoss_Walk : StateMachineBehaviour
{
    Transform player;
    Rigidbody2D rb;
    EnemyStats stats;
    bool isKnockBack;
    EnemyBase enemyBase;
    EnemyBoss enemyBoss;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameManager.instance.player.transform;
        rb = animator.GetComponent<Rigidbody2D>();
        enemyBase = animator.GetComponent<EnemyBase>();
        enemyBoss = animator.GetComponent<EnemyBoss>();
        stats = enemyBase.Stats;
        isKnockBack = enemyBase.IsKnockBack;

        enemyBoss.ActivateLandingIndicator(false);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBase.Flip();
        enemyBase.ApplyMovement();
        enemyBoss.ShootTimer();
        enemyBoss.SlimeDropTimer();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       
    }
}
