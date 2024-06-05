using UnityEngine;

public class SlimeBoss_Land : StateMachineBehaviour
{
    EnemyBoss enemyBoss;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (enemyBoss == null) enemyBoss = animator.GetComponent<EnemyBoss>();
        enemyBoss.transform.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<EnemyBoss>().transform.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
    }
}
