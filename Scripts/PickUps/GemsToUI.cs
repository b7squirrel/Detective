using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemsToUI : MonoBehaviour
{
    public static GemsToUI instance;
    Vector2 targetScrnPos;
    Vector2 targetWorldPos;
    [SerializeField] float moveSpeed;
    [SerializeField] float anticTime;
    [SerializeField] Transform gemUI;
    Character character;
    Coroutine moveCo;
    void Awake()
    {
        instance = this;
        targetScrnPos = transform.position;
        targetWorldPos = Camera.main.ScreenToWorldPoint(targetScrnPos);

        character = FindObjectOfType<Character>();
    }

    public void MoveGem(Transform gem, int experience)
    {
        moveCo = StartCoroutine(MoveCo(gem, experience));
    }

    IEnumerator MoveCo(Transform gem, int experience)
    {
        yield return StartCoroutine(MoveTowardsUi(gem));
        yield return StartCoroutine(OnArrival(gem, experience));
    }

    IEnumerator MoveTowardsUi(Transform gemTransform)
    {
        while (targetWorldPos.y - gemTransform.position.y > .2f)
        {
            gemTransform.position -= new Vector3(0, -Time.deltaTime * moveSpeed, 0);
            yield return null;
        }

        // 확대되었다가 축소, 사라짐
        // 사운드
        // 플레이어에게 경험치를 전달
    }

    IEnumerator OnArrival(Transform gem ,int experience)
    {
        yield return null;
        Debug.Log("경험치는 " + experience);
        character.level.AddExperience(experience);
        Destroy(gem.gameObject);
    }
}
