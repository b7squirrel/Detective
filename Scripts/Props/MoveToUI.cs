using System.Collections;
using UnityEngine;

public class MoveToUI : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] AudioClip hitSound;
    [SerializeField] float waitingTimeBeforeMoving;

    bool isMovementTriggered; // 트리거 되어서 이동 중인지를 알려주는 변수
    ShadowHeight shadowHeight;
    Vector2 targetScrnPos;
    Vector2 targetWorldPos;

    CoinManager coinManager;

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
}
