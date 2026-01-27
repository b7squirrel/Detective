using System.Collections;
using UnityEngine;
/// <summary>
/// MoveToUI의 2중 안전장치
/// 랜덤 딜레이: 각 오브젝트가 0~0.15초 사이 랜덤하게 늦게 출발
/// 정적 쿨다운: 모든 오브젝트가 공유하는 쿨다운 (0.05초)
/// 사운드 매니져의 3중 안전 장치와 협력하여 여러 동전이 동시에 생성되어도 소리가 겹치지 않고 자연스럽게 재생
/// </summary>
enum TypeOfMoveToUI { None, Coin, Cristal }
public class MoveToUI : MonoBehaviour
{
    [Header("오브젝트 타입")]
    [SerializeField] TypeOfMoveToUI typeOfMoveToUI;

    [Header("2라면 실제 획득하는 수의 절만만 UI로 생성")]
    [SerializeField] int divideFactor;

    [Header("World Object")]
    [SerializeField] float moveSpeed;
    [SerializeField] AudioClip hitSound;
    [SerializeField] float waitingTimeBeforeMoving;
    [SerializeField] float randomDelayRange = 0.15f; // 랜덤 딜레이 범위

    bool isMovementTriggered; // 트리거가 되어 이동 시작했는지 알려주는 변수
    ShadowHeight shadowHeight;
    Vector2 targetScrnPos;
    Vector2 targetWorldPos;
    CoinManager coinManager;
    CristalManager cristalManager;

    [Header("UI Object")]
    [SerializeField] Canvas targetCanvas; // Screen Space - Overlay 캔버스
    [SerializeField] RectTransform uiPrefab;

    [Header("값 설정")]
    int valuePerProp = 1; // 이 오브젝트가 대표하는 실제 값

    SmoothScreenPositionController smoothScreenPositionController;

    // 모든 동전이 공유하는 정적 변수 (쿨다운 관리)
    private static float lastCoinSoundTime = -999f;
    private static float coinSoundInterval = 0.05f;

    void OnEnable()
    {
        isMovementTriggered = false;
        shadowHeight = GetComponent<ShadowHeight>();
        moveSpeed += Random.Range(-8f, 8f);

        // ⭐ 기본값 리셋 (오브젝트 풀링 시 중요!)
        valuePerProp = 1;

        if (coinManager == null)
        {
            coinManager = GameManager.instance.GetComponent<CoinManager>();
        }
        if (cristalManager == null)
        {
            cristalManager = GameManager.instance.GetComponent<CristalManager>();
        }
    }

    IEnumerator Trigger()
    {
        // 랜덤 딜레이로 도착 시간 분산
        float randomDelay = Random.Range(0f, randomDelayRange);
        yield return new WaitForSeconds(waitingTimeBeforeMoving + randomDelay);

        isMovementTriggered = true;

        if (smoothScreenPositionController == null)
            smoothScreenPositionController = GetComponent<SmoothScreenPositionController>();

        SetTargetPos();
        smoothScreenPositionController.MoveToScreenPosition(targetScrnPos);

        while (smoothScreenPositionController.IsMoving())
        {
            yield return null;
        }

        // ⭐ 도착 시 UI만 업데이트 (데이터는 이미 저장됨)
        if (typeOfMoveToUI == TypeOfMoveToUI.Coin)
        {
            coinManager.UpdateCoinUIOnly(valuePerProp);
        }
        else if (typeOfMoveToUI == TypeOfMoveToUI.Cristal)
        {
            cristalManager.UpdateCristalUIOnly(valuePerProp);
        }

        // 정적 쿨다운으로 소리 재생 제어
        if (Time.time - lastCoinSoundTime >= coinSoundInterval)
        {
            SoundManager.instance.PlaySoundWith(hitSound, 1f, false, .1f);
            lastCoinSoundTime = Time.time;
        }

        gameObject.SetActive(false);
    }

    void SetTargetPos()
    {
        if (typeOfMoveToUI == TypeOfMoveToUI.Coin)
        {
            targetScrnPos = GameManager.instance.CoinUIPosition.transform.position;
        }
        else if (typeOfMoveToUI == TypeOfMoveToUI.Cristal)
        {
            targetScrnPos = GameManager.instance.CristalUIPosition.transform.position;
        }
        targetWorldPos = Camera.main.ScreenToWorldPoint(targetScrnPos);
    }

    void Update()
    {
        if (shadowHeight.IsDone && isMovementTriggered == false)
        {
            StartCoroutine(Trigger());
        }
    }

    // ⭐ MultiPropCreator에서 호출할 public 메서드
    public void SetValuePerProp(int value)
    {
        valuePerProp = value;
    }
}