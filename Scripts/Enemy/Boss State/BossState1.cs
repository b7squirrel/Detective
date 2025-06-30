using UnityEngine;

/// <summary>
/// State1은 Walk로 설정
/// </summary>
public class BossState1 : StateMachineBehaviour
{
    [SerializeField] float stateDuration;
    float stateTimer;
    EnemyBase enemyBase;
    EnemyBoss enemyBoss;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBase = animator.GetComponent<EnemyBase>();
        enemyBoss = animator.GetComponent<EnemyBoss>();
        enemyBoss.ActivateLandingIndicator(false);

        enemyBoss.DisplayCurrentState("헬멧 슬라임 걷기");
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
        enemyBase.Flip();
        enemyBase.ApplyMovement();
        enemyBoss.ShootTimer();
        enemyBoss.SlimeDropTimer();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
