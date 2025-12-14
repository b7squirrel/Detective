using UnityEngine;

public class AnimationEventManager : MonoBehaviour
{
    public bool IsAnimationComplete { get; private set; }
    public void IsAnimComplete()
    {
        IsAnimationComplete = true;
    }
    public void isAnimStarting()
    {
        IsAnimationComplete = false;
    }

}
