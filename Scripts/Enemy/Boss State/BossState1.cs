using UnityEngine;

/// <summary>
/// State1은 Walk로 설정
/// </summary>
public class BossState1 : StateMachineBehaviour
{
    [SerializeField] float stateDuration;
    float stateTimer;
    EnemyBoss enemyBoss;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss = animator.GetComponent<EnemyBoss>();
        enemyBoss.ActivateLandingIndicator(false);

        // 개별 Enter
        enemyBoss.ExecuteState1Enter();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 공통 Update
        if (stateTimer < stateDuration)
        {
            stateTimer += Time.deltaTime;
        }
        else
        {
            stateTimer = 0f;
            enemyBoss.SetRandomState();
        }

        // 개별 Update
        enemyBoss.ExecuteState1Update();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 개별 Exit
        enemyBoss.ExecuteState1Exit();
    }
}
