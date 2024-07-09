using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistributeEffects : MonoBehaviour
{
    [SerializeField] float radius = 100.0f; // 반지름 (UI 좌표계에서의 거리 단위)
    [SerializeField] int numberOfEffects; // 생성할 랜덤 위치의 수
    [SerializeField] List<Vector2> randomEffects; // 랜덤 위치를 담을 리스트
    [SerializeField] GameObject EffectPrefab; // UI 요소의 프리팹

    void Start()
    {
        randomEffects = new List<Vector2>();
        StartCoroutine(GenEffects());
    }
    
    Vector2 GetRandomPoint()
    {
        Vector2 randomPoint = Random.insideUnitCircle * radius;
        return randomPoint;
    }
    IEnumerator GenEffects()
    {
        int count = numberOfEffects;
        while(count > 0)
        {
            GameObject effect = Instantiate(EffectPrefab, transform);
            SetEffectOnRandomPoint(effect);

            count--;
            yield return new WaitForSeconds(.03f);
        }
    }
    public void SetEffectOnRandomPoint(GameObject _effect)
    {
        RectTransform rectTransform = _effect.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = GetRandomPoint();
        rectTransform.localScale = UnityEngine.Random.Range(.3f, 1f) * Vector2.one;
    }
}