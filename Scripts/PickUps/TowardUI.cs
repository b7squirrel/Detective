using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowardUI : MonoBehaviour
{
    Vector2 targetScrnPos;
    Vector2 targetWorldPos;
    [SerializeField] float moveSpeed;
    [SerializeField] float anticTime;
    Coroutine anticCoroutine;

    void OnEnable()
    {
        targetScrnPos = GameManager.instance.CoinUIPosition.transform.position;
        targetWorldPos = Camera.main.ScreenToWorldPoint(targetScrnPos);
        anticCoroutine = StartCoroutine(Antic()); // 활성화 되면 바로 UI쪽 반대 방향으로 약간의 예비동작을 하게 된다. 
    }

    void OnDisable()
    {
        if(anticCoroutine != null) StopCoroutine(anticCoroutine);
    }

    void Update()
    {

    }

    void MoveTowardsUi()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime + (0.5f * 4f * Time.deltaTime * Time.deltaTime));
        if (Vector2.Distance((Vector2)transform.position, targetWorldPos) < .2f)
        {
            // 확대되었다가 축소, 사라짐
            // 사운드
            // 플레이어에게 경험치를 전달
        }
    }
    IEnumerator Antic()
    {
        float currentTime = 0;
        while (currentTime < anticTime)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);
            currentTime += Time.deltaTime;
            yield return null;
        }
    }
}
