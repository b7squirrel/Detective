using System.Collections;
using DG.Tweening;
using UnityEngine;

public class UIShaker : MonoBehaviour
{
    public IEnumerator ShakeUI(float duration, float magnitude)
    {
        RectTransform uiTransform = GetComponent<RectTransform>();
        Vector3 originalPosition = uiTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            uiTransform.anchoredPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        uiTransform.anchoredPosition = originalPosition;
    }
    public void Shake()
    {
        StartCoroutine(ShakeUI(.5f, .1f));
    }
}
