using UnityEngine;

public class BossMossPowder : StateMachineBehaviour
{
    Transform player;
    Rigidbody2D rb;
    EnemyStats stats;
    bool isKnockBack;
    EnemyBase enemyBase;
    EnemyBoss enemyBoss;
    BossBase bossBase;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameManager.instance.player.transform;
        rb = animator.GetComponent<Rigidbody2D>();
        enemyBase = animator.GetComponent<EnemyBase>();
        enemyBoss = animator.GetComponent<EnemyBoss>();
        stats = enemyBase.Stats;
        isKnockBack = enemyBase.IsKnockBack;

        enemyBase = animator.GetComponent<EnemyBase>();
        bossBase = animator.GetComponent<BossBase>();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
