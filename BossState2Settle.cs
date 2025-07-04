using UnityEngine;

public class BossState2Settle : StateMachineBehaviour
{
    Rigidbody2D rb;
    EnemyBoss enemyBoss;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb = animator.GetComponent<Rigidbody2D>();
        enemyBoss = animator.GetComponent<EnemyBoss>();

        enemyBoss.ExecuteState2SettleEnter();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss.ExecuteState2SettleUpdate();

        rb.velocity = Vector2.zero;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss.ExecuteState2SettleExit();
    }
}
