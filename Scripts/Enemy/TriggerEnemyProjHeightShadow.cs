using UnityEngine;

public enum enemyProjectileType {parabolaTime, parabolaHeight, parabolaVelocity}

public class TriggerEnemyProjHeightShadow : MonoBehaviour
{
    [Header("투사체 종류")]
   [SerializeField] enemyProjectileType projectileType;
    [Header("포물선 시간형")]
    [SerializeField] float duration; // 날아가는 시간

    [Header("포물선 높이형")]
    [SerializeField] float maxHeight; // 최고 높이

    [Header("직진 속도형")]
    [SerializeField] float horizontalvelocity; // 속도
    [SerializeField] float verticalvelocity; // 속도

    void OnEnable()
    {
        ShadowHeightProjectile shadowHeightProj = GetComponent<ShadowHeightProjectile>();
        Vector2 dir = (GameManager.instance.player.transform.position - transform.position).normalized;
        shadowHeightProj.Initialize(horizontalvelocity * dir, verticalvelocity);
    }
}
