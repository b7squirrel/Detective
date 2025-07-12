using UnityEngine;

public class BossState1Settle : StateMachineBehaviour
{
    Rigidbody2D rb;
    EnemyBoss enemyBoss;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb = animator.GetComponent<Rigidbody2D>();
        enemyBoss = animator.GetComponent<EnemyBoss>();

        // 플레이어에게 밀리지 않도록
        enemyBoss.SetMovable(false);

        enemyBoss.ExecuteState1SettleEnter();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss.ExecuteState1SettleUpdate();

        rb.velocity = Vector2.zero;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss.SetMovable(true);
        
        enemyBoss.ExecuteState1SettleExit();
    }
}
