using UnityEngine;

public class BossState3Antic : StateMachineBehaviour
{
    EnemyBoss enemyBoss;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss = animator.GetComponent<EnemyBoss>();
        enemyBoss.ExecuteState3AnticEnter();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss.ExecuteState3AnticUpdate();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss.ExecuteState3AnticExit();
    }
}
