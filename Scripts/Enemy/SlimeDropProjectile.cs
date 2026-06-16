using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SlimeDropProjectile : MonoBehaviour
{
    [SerializeField] Transform jumpSprite;
    [SerializeField] GameObject slimeDropPrefab;

    SlimeDropManager slimeDropManager;
    ShadowHeightProjectile shadowHeightProjectile;

    public UnityEvent onDoneEvent;

    void Awake()
    {
        shadowHeightProjectile = GetComponent<ShadowHeightProjectile>();

        // 착지 시 Bounce 호출 (divisionFactor는 상관없음, 바로 Done 처리됨)
        shadowHeightProjectile.onGroundHitEvent.AddListener(() => shadowHeightProjectile.Bounce(2f));

        // Done 시 ProjectileDone 호출
        shadowHeightProjectile.onDone.AddListener(ProjectileDone);
    }

    public void InitProjectile(Vector2 startPoint, Vector2 endPoint, float speed, SlimeDropManager manager)
    {
        slimeDropManager = manager;
        transform.position = startPoint;

        // ⭐ 랜덤 속도 오프셋 추가
        float randomOffset = Random.Range(0f, speed * 2f);
        float finalSpeed = speed + randomOffset;

        Vector2 direction = (endPoint - startPoint).normalized;
        Vector2 groundVelocity = direction * finalSpeed;
        float verticalVelocity = 50f;

        shadowHeightProjectile.Initialize(groundVelocity, verticalVelocity);
    }

    void ProjectileDone()
    {
        onDoneEvent?.Invoke();
        gameObject.SetActive(false);
    }

    public void Event_DropNormalSlimeDrop()
    {
        if (slimeDropManager == null)
        {
            Debug.LogError("SlimeDropManager 참조 없음!");
            return;
        }
        slimeDropManager.DropObjectOnLanding(transform.position);
    }
}