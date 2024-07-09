using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistributeEffects : MonoBehaviour
{
    [SerializeField] float radius = 100.0f; // ������ (UI ��ǥ�迡���� �Ÿ� ����)
    [SerializeField] int numberOfEffects; // ������ ���� ��ġ�� ��
    [SerializeField] List<Vector2> randomEffects; // ���� ��ġ�� ���� ����Ʈ
    [SerializeField] GameObject EffectPrefab; // UI ����� ������

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