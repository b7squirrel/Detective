using System.Collections;
using UnityEngine;
public class MoveToUI : MonoBehaviour
{
    [Header("World Object")]
    [SerializeField] float moveSpeed;
    [SerializeField] AudioClip hitSound;
    [SerializeField] float waitingTimeBeforeMoving;
    bool isMovementTriggered; // 트리거 되어서 이동 중인지를 알려주는 변수
    ShadowHeight shadowHeight;
    Vector2 targetScrnPos;
    Vector2 targetWorldPos;
    CoinManager coinManager;

    [Header("UI Object")]
    [SerializeField] Canvas targetCanvas; // Screen Space - Overlay 캔버스
    [SerializeField] RectTransform uiPrefab;
    void OnEnable()
    {
        isMovementTriggered = false;
        shadowHeight = GetComponent<ShadowHeight>();
        SetTargetPos();
        moveSpeed += Random.Range(-8f, 8f);
        if (coinManager == null)
            coinManager = GameManager.instance.GetComponent<CoinManager>();
    }
    IEnumerator Trigger()
    {
        yield return new WaitForSeconds(waitingTimeBeforeMoving);
        isMovementTriggered = true;
    }
    void SetTargetPos()
    {
        targetScrnPos = GameManager.instance.CoinUIPosition.transform.position;
        targetWorldPos = Camera.main.ScreenToWorldPoint(targetScrnPos);
    }
    void Update()
    {
        if (shadowHeight.IsDone)
        {
            StartCoroutine(Trigger());
        }
        if (isMovementTriggered == false)
            return;
        SetTargetPos();
        transform.position = Vector2.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime + (0.5f * 4f * Time.deltaTime * Time.deltaTime));
        if (Vector2.Distance((Vector2)transform.position, targetWorldPos) > .2f)
            return;
        coinManager.updateCurrentCoinNumbers(1);
        SoundManager.instance.PlaySoundWith(hitSound, 1f, false, .1f);
        gameObject.SetActive(false);
    }

    RectTransform SpawnUIAtWorldPosition(Vector3 worldPosition)
    {
        // 1. 월드 좌표를 스크린 좌표로 변환
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);

        // 2. 스크린 좌표를 캔버스의 로컬 좌표로 변환
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetCanvas.GetComponent<RectTransform>(),
            screenPoint,
            null, // Screen Space - Overlay에서는 카메라를 null로 설정
            out localPoint);

        // 3. UI 오브젝트 생성 및 위치 설정
        RectTransform spawnedUI = Instantiate(uiPrefab, targetCanvas.transform);
        spawnedUI.localPosition = localPoint;

        return spawnedUI;
    }
}