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

    SmoothScreenPositionController smoothScreenPositionController;
    void OnEnable()
    {
        isMovementTriggered = false;
        shadowHeight = GetComponent<ShadowHeight>();
        moveSpeed += Random.Range(-8f, 8f);
        if (coinManager == null)
            coinManager = GameManager.instance.GetComponent<CoinManager>();
    }
    IEnumerator Trigger()
    {
        yield return new WaitForSeconds(waitingTimeBeforeMoving);
        isMovementTriggered = true;

        //  Smooth Screen Position Controller에서 물체 이동 제어
        if (smoothScreenPositionController == null) smoothScreenPositionController = GetComponent<SmoothScreenPositionController>();

        SetTargetPos();
        smoothScreenPositionController.MoveToScreenPosition(targetScrnPos);

        while (smoothScreenPositionController.IsMoving())
        {
            yield return null;
        }

        coinManager.updateCurrentCoinNumbers(1);
        SoundManager.instance.PlaySoundWith(hitSound, 1f, false, .1f);
        gameObject.SetActive(false);
    }
    void SetTargetPos()
    {
        targetScrnPos = GameManager.instance.CoinUIPosition.transform.position;
        targetWorldPos = Camera.main.ScreenToWorldPoint(targetScrnPos);
    }
    void Update()
    {
        if (shadowHeight.IsDone && isMovementTriggered == false)
        {
            StartCoroutine(Trigger());
        }
    }
}