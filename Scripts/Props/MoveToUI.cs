using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToUI : MonoBehaviour
{
    bool TriggerMoving;
    Vector2 targetScrnPos;
    Vector2 targetWorldPos;
    Rigidbody2D rb;
    [SerializeField] float moveSpeed;
    [SerializeField] float knockBackTime;
    [SerializeField] float knockBackForce;
    [SerializeField] GameObject debuggingDot;
    [SerializeField] AudioClip hitSound;

    void OnEnable()
    {
        TriggerMoving = false;
        SetTargetPos();

        rb = GetComponent<Rigidbody2D>();
        Vector2 dir = (targetWorldPos - (Vector2)transform.position).normalized;

        StartCoroutine(KnockBack(dir));
    }

    void SetTargetPos()
    {
        targetScrnPos = GameManager.instance.CoinUIPosition.transform.position;
        targetWorldPos = Camera.main.ScreenToWorldPoint(targetScrnPos);
    }

    IEnumerator KnockBack(Vector2 dir)
    {
        Vector2 knockBackDir = ((Vector2)transform.position - targetScrnPos).normalized;
        rb.AddForce(knockBackDir * knockBackForce, ForceMode2D.Impulse);
        
        yield return new WaitForSeconds(knockBackTime);
        rb.velocity = Vector2.zero;
        TriggerMoving = true;
    }

    void Update()
    {
        if (TriggerMoving)
        {
            SetTargetPos();
            transform.position = Vector2.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);
            if (Vector2.Distance((Vector2)transform.position, targetWorldPos) < .2f)
            {
                FindObjectOfType<Coins>().Add(1);
                SoundManager.instance.Play(hitSound);
                Destroy(gameObject);
            }
        }
    }
}
