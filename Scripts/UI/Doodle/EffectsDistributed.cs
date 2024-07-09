using UnityEngine;

public class EffectsDistributed : MonoBehaviour
{
    DistributeEffects distributeEffects;

    public void AnimDone()
    {
        if (distributeEffects == null)
        {
            distributeEffects = GetComponentInParent<DistributeEffects>();
            if (distributeEffects == null) return;
        }
        distributeEffects.SetEffectOnRandomPoint(this.gameObject);
    }
}