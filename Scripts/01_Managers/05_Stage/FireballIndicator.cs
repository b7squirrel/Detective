using System.Collections;
using UnityEngine;

public class FireballIndicator : MonoBehaviour
{
    [SerializeField] SpriteRenderer sr;
    [SerializeField] Color startColor = new Color(1f, 0.3f, 0f, 0.3f);  // 주황 반투명
    [SerializeField] Color endColor = new Color(1f, 0f, 0f, 0.8f);      // 빨강 불투명

    [SerializeField] float minBlinkInterval = 0.4f;  // 처음 깜빡임 간격 (느림)
    [SerializeField] float maxBlinkInterval = 0.08f; // 최대 깜빡임 간격 (빠름)

    [SerializeField] float startScale = 0.3f;
    [SerializeField] float endScale = 1.5f;

    public void Show(float duration)
    {
        StartCoroutine(ShowCo(duration));
    }

    IEnumerator ShowCo(float duration)
    {
        float elapsed = 0f;

        // duration 동안 크기/색상 변화 + 깜빡임
        // duration 이후에도 깜빡임 유지 (Hide() 호출까지)
        while (true)
        {
            // duration 안에서만 크기/색상 진행
            float progress = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, progress);
            if (sr != null)
                sr.color = Color.Lerp(startColor, endColor, progress);

            // 깜빡임 간격: duration 안에서는 점점 빨라지고, 이후에는 최대 속도 유지
            float blinkInterval = Mathf.Lerp(minBlinkInterval, maxBlinkInterval, progress);
            float halfBlink = blinkInterval * 0.5f;

            // 켜기
            if (sr != null) sr.enabled = true;
            yield return new WaitForSeconds(halfBlink);
            elapsed += halfBlink;

            // 끄기
            if (sr != null) sr.enabled = false;
            yield return new WaitForSeconds(halfBlink);
            elapsed += halfBlink;
        }
    }

    public void Hide()
    {
        StopAllCoroutines();
        if (sr != null) sr.enabled = true; // 꺼진 상태로 풀에 반환되지 않도록
        gameObject.SetActive(false);
    }
}