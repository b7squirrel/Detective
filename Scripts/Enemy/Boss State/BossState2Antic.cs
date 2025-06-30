using UnityEngine;

public class BossState2Antic : StateMachineBehaviour
{
    EnemyBoss enemyBoss;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss = animator.GetComponent<EnemyBoss>();
        enemyBoss.ExecuteState2AnticEnter();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss.ExecuteState2AnticUpdate();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss.ExecuteState2AnticExit();
    }
}
