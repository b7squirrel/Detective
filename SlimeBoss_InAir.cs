using UnityEngine;

public class SlimeBoss_InAir : StateMachineBehaviour
{
    Vector2 target;
    float moveSpeed;
    float elapsedTIme;
    Vector2 startPos; // 점프를 시작하는 지점
    [SerializeField] LayerMask noCollisionLayer;
    [SerializeField] float duration; // 공중에 떠 있는 시간

    WallManager wallManager;
    Transform bossTransform;
    EnemyBoss enemyBoss;


    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // collider는 aniamtion event로 껐음
        animator.GetComponent<EnemyBoss>().IsInAir = true;
        target = Player.instance.transform.position;
        moveSpeed = animator.GetComponent<EnemyBoss>().moveSpeedInAir;
        elapsedTIme = 0f;
        animator.GetComponent<EnemyBoss>().SetLayer("InAir");
        bossTransform = animator.GetComponent<Transform>();
        startPos = bossTransform.position;
        enemyBoss = animator.GetComponent<EnemyBoss>();

        enemyBoss.ActivateLandingIndicator(true);
        Debug.Log("Start In AIR");
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (elapsedTIme < duration)
        {
            float t = Mathf.Clamp01(elapsedTIme / duration);
            bossTransform.position = Vector2.Lerp(startPos, target, t);
            elapsedTIme += Time.deltaTime;
        }
        else
        {
            bossTransform.position = target;
            animator.SetTrigger("IsClose");
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
