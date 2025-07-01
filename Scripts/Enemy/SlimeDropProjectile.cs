using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 플레이어의 주변으로 날아가는 적 투사체
/// </summary>
public class SlimeDropProjectile : MonoBehaviour
{
    [SerializeField] Transform jumpSprite;
    [SerializeField] GameObject slimeDropPrefab; // 투사체가 목표물에 도착하면 드롭될 점액프리펩
    public UnityEvent onDoneEvent; // 투사체 죽음 이벤트

    public void InitProjectile(Vector2 startPoint, Vector2 endPoint, float duration)
    {
        StartCoroutine(MoveObjectCo(startPoint, endPoint, duration));
    }

    IEnumerator MoveObjectCo(Vector2 startPoint, Vector2 endPoint, float speed)
    {
        float elapsedTime = 0f;
        float distance = Vector3.Distance(startPoint, endPoint);
        float duration = distance / speed;

        while (elapsedTime < duration)
        {
            // 스톱워치로 시간이 멈춰 있다면 이동하지 않음
            if (GameManager.instance.fieldItemEffect.IsStopedWithStopwatch()) yield return null;
            if (Time.timeScale == 0) yield return null;

            // 수평 이동
            float t = elapsedTime / duration;
            transform.position = Vector3.Lerp(startPoint, endPoint, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 마지막 위치를 정확히 도착 지점으로 설정
        transform.position = endPoint;

        // 점액을 드롭하고 자신은 파괴
        ProjectileDone();
    }
    void ProjectileDone()
    {
        onDoneEvent?.Invoke();
        gameObject.SetActive(false); // 공통적으로 들어가야 하니 함수를 따로 빼지 않고 여기에 넣어 버리자
    }

    // 유니티 이벤트에 붙일 함수들
    public void Event_DropNormalSlimeDrop()
    {
        Instantiate(slimeDropPrefab, transform.position, Quaternion.identity);
    }
}
