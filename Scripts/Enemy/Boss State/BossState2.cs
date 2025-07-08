using UnityEngine;

public class BossState2 : StateMachineBehaviour
{
    EnemyBoss enemyBoss;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 공통 Enter
        enemyBoss = animator.GetComponent<EnemyBoss>();

        // 개별 Enter
        enemyBoss.ExecuteState2Enter();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 개별 Update
        enemyBoss.ExecuteState2Update();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 개별 Exit
        enemyBoss.ExecuteState2Exit();
    }
}
