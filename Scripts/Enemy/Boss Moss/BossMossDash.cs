using UnityEngine;

public class BossMossDash : StateMachineBehaviour
{
    [SerializeField] float dashSpeed;

    Transform player;
    Rigidbody2D rb;
    EnemyStats stats;
    bool isKnockBack;
    EnemyBase enemyBase;
    EnemyBoss enemyBoss;
    BossBase bossBase;

    Vector2 targetPoint; // 대쉬할 지점

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

        targetPoint = player.position;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector2 dirVec = targetPoint - (Vector2)animator.transform.position;
        Vector2 nextVec = dashSpeed * Time.fixedDeltaTime * dirVec.normalized;
        rb.MovePosition((Vector2)rb.transform.position + nextVec);
        rb.velocity = Vector2.zero;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
