using System.Collections;
using UnityEngine;

public class MoveToUI : MonoBehaviour
{
    bool TriggerMoving;
    ShadowHeight shadowHeight;
    Vector2 targetScrnPos;
    Vector2 targetWorldPos;
    Rigidbody2D rb;
    [SerializeField] float moveSpeed;
    [SerializeField] AudioClip hitSound;

    CoinManager coinManager;

    void OnEnable()
    {
        TriggerMoving = false;
        shadowHeight = GetComponent<ShadowHeight>();
        SetTargetPos();

        rb = GetComponent<Rigidbody2D>();
        Vector2 dir = (targetWorldPos - (Vector2)transform.position).normalized;
        moveSpeed += Random.Range(-8f, 8f);

        if (coinManager == null)
            coinManager = GameManager.instance.GetComponent<CoinManager>();
    }
    IEnumerator Trigger()
    {
        yield return new WaitForSeconds(1.5f);
        TriggerMoving = true;
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

        if (TriggerMoving)
        {
            SetTargetPos();
            transform.position = Vector2.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime + (0.5f * 4f * Time.deltaTime * Time.deltaTime));
            if (Vector2.Distance((Vector2)transform.position, targetWorldPos) < .2f)
            {
                coinManager.updateCurrentCoinNumbers(1);
                //FindObjectOfType<Coins>().Add(1);
                //SoundManager.instance.Play(hitSound);
                //CoinUI.instance.PopCoinIcon();
                Destroy(gameObject);
            }
        }
    }
}
