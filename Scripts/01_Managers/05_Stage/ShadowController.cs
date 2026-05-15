using UnityEngine;

public class ShadowController : MonoBehaviour
{
    [SerializeField] GameObject shadowDefault;  // Shadow/Sprite
    [SerializeField] GameObject shadowOnIce;    // Shadow/Sprite on Ice

    void OnEnable()
    {
        ApplyShadow(StageGroundEffectManager.IsIceStage);
    }

    public void ApplyShadow(bool isIce)
    {
        if (shadowDefault != null) shadowDefault.SetActive(!isIce);
        if (shadowOnIce != null) shadowOnIce.SetActive(isIce);
    }
}