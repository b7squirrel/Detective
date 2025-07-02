using UnityEngine;

public class BossState2Antic : StateMachineBehaviour
{
    EnemyBoss enemyBoss;

    DialogBubble dialogBubble;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss = animator.GetComponent<EnemyBoss>();
        enemyBoss.ExecuteState2AnticEnter();

        // 말풍선 활성화
        if (dialogBubble == null) dialogBubble = animator.GetComponent<DialogBubble>();
        dialogBubble = enemyBoss.GetComponent<DialogBubble>();
        dialogBubble.InitDialogBubble(animator.transform);
        dialogBubble.SetBubbleActive(true);

        dialogBubble.SetDialogText(enemyBoss.GetDialog(1)); // state1 : 0, state2 : 1, state3 : 2
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss.ExecuteState2AnticUpdate();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 말풍선 비활성화
        dialogBubble.SetBubbleActive(false);

        enemyBoss.ExecuteState2AnticExit();
    }
}
