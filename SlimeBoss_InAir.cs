using UnityEngine;

public class SlimeBoss_InAir : StateMachineBehaviour
{
    Vector2 target;
    float moveSpeed;
    [SerializeField] LayerMask noCollisionLayer;

    WallManager wallManager;
    Transform bossTransform;
    EnemyBoss enemyBoss;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // collider는 aniamtion event로 껐음
        animator.GetComponent<EnemyBoss>().IsInAir = true;
        target = Player.instance.transform.position;
        moveSpeed = animator.GetComponent<EnemyBoss>().moveSpeedInAir;
        animator.GetComponent<EnemyBoss>().SetLayer("InAir");
        bossTransform = animator.GetComponent<Transform>();
        enemyBoss = animator.GetComponent<EnemyBoss>();

        enemyBoss.ActivateLandingIndicator(true);
        Debug.Log("Start In AIR");
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss.RePosition();
        bossTransform.position = Vector2.MoveTowards(bossTransform.position, target, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(bossTransform.position, target) < 2f)
        {
            animator.SetTrigger("IsClose");
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
