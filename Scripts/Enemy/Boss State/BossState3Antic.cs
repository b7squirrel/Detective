using UnityEngine;

public class BossState3Antic : StateMachineBehaviour
{
    EnemyBoss enemyBoss;
    DialogBubble dialogBubble;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss = animator.GetComponent<EnemyBoss>();
        enemyBoss.ExecuteState3AnticEnter();

        // 말풍선 활성화
        if (dialogBubble == null) dialogBubble = animator.GetComponent<DialogBubble>();
        dialogBubble = enemyBoss.GetComponent<DialogBubble>();
        dialogBubble.InitDialogBubble(enemyBoss.GetDialogBubblePoint());
        dialogBubble.SetBubbleActive(true);

        dialogBubble.SetDialogText(enemyBoss.GetDialog(2)); // state1 : 0, state2 : 1, state3 : 2
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBoss.ExecuteState3AnticUpdate();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 말풍선 비활성화
        dialogBubble.SetBubbleActive(false);

        enemyBoss.ExecuteState3AnticExit();
    }
}
