using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

/// <summary>
/// 멈춰 있는 역할만 함
/// </summary>
public class BossStateSettle : StateMachineBehaviour
{
    Rigidbody2D rb;
    EnemyBoss enemyBoss;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb = animator.GetComponent<Rigidbody2D>();
        enemyBoss = animator.GetComponent<EnemyBoss>();

        enemyBoss.ExecuteStateSettleEnter();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss.ExecuteStateSettleUpdate();

        rb.velocity = Vector2.zero;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss.ExecuteStateSettleExit();
    }
}
