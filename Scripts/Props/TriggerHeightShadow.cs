using UnityEngine;

public class TriggerHeightShadow : MonoBehaviour
{
    ShadowHeight shadowHeight;
    [SerializeField] float verticalVel;
    [SerializeField] Vector2 groundVel;
    void OnEnable()
    {
        shadowHeight = GetComponent<ShadowHeight>();
        shadowHeight.Initialize(groundVel, verticalVel);
    }
}