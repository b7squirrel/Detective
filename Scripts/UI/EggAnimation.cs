using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggAnimation : MonoBehaviour
{
    // animation event
    // Egg Panel Manager에 애니메이션이 끝났음을 알리고 자신을 비활성화
    public void AnimFinished()
    {
        GameManager.instance.eggPanelManager.EggAnimFinished();
        GameManager.instance.eggPanelManager.EggImageUp(false);
    }
}
