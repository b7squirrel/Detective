using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 개별 버프 아이콘의 동작을 담당.
/// Timer Ring의 Fill Amount로 남은 시간을 표시.
/// </summary>
public class BuffIconUI : MonoBehaviour
{
    [SerializeField] GameObject multiplierGroup;      // Multiplier 오브젝트
    [SerializeField] TextMeshProUGUI multiplierText;  // Multiplier Text
    [SerializeField] GameObject multiplierCoin;       // Multiplier Coin 오브젝트
    [SerializeField] GameObject multiplierExp;        // Multiplier Exp 오브젝트
    [SerializeField] Image timerRing;                 // Timer Ring Image (Fill Method: Radial 360)
    [SerializeField] AudioClip popSound;

    float totalDuration;
    float remainingTime;
    bool isRunning;

    public FieldBuffType BuffType { get; private set; }

    /// <summary>
    /// BuffDisplayManager에서 아이콘 생성 시 호출
    /// </summary>
    public void Init(FieldBuffType buffType, float duration)
    {
        BuffType = buffType;
        totalDuration = duration;
        remainingTime = duration;
        isRunning = true;

        SetupMultiplierDisplay(buffType);
        UpdateMultiplierText(buffType);

        if (timerRing != null)
            timerRing.fillAmount = 0f;
    }

    /// <summary>
    /// 같은 버프를 다시 획득했을 때 타이머만 리셋
    /// </summary>
    public void ResetTimer(float duration)
    {
        totalDuration = duration;
        remainingTime = duration;
        isRunning = true;

        UpdateMultiplierText(BuffType);

        if (timerRing != null)
            timerRing.fillAmount = 0f;

        // 팝 애니메이션
        StopAllCoroutines();
        StartCoroutine(PopAnimation());
    }

    void Update()
    {
        if (!isRunning) return;

        remainingTime -= Time.deltaTime;

        // 0에서 시작해서 시계방향으로 채워지다가, 시간이 끝나면 꽉 참
        if (timerRing != null)
            timerRing.fillAmount = Mathf.Clamp01(1f - (remainingTime / totalDuration));

        if (remainingTime <= 0f)
        {
            isRunning = false;
            // BuffDisplayManager가 FieldItemEffect 이벤트로 제거하므로 여기선 아무것도 안 함
        }
    }

    /// <summary>
    /// 버프 타입에 따라 Multiplier 하위 오브젝트 활성/비활성
    /// </summary>
    void SetupMultiplierDisplay(FieldBuffType buffType)
    {
        if (multiplierGroup == null) return;

        bool showMultiplier = (buffType == FieldBuffType.DoubleExp || buffType == FieldBuffType.DoubleCoin);
        multiplierGroup.SetActive(showMultiplier);

        if (!showMultiplier) return;

        // Coin / Exp 아이콘 중 해당하는 것만 표시
        // if (multiplierCoin != null)
        //     multiplierCoin.SetActive(buffType == FieldBuffType.DoubleCoin);
        // if (multiplierExp != null)
        //     multiplierExp.SetActive(buffType == FieldBuffType.DoubleExp);
    }

    /// <summary>
    /// 현재 배율에 맞게 텍스트 업데이트
    /// </summary>
    void UpdateMultiplierText(FieldBuffType buffType)
    {
        if (multiplierText == null) return;

        if (buffType == FieldBuffType.DoubleExp)
        {
            multiplierText.text = $"x{(int)FieldItemEffect.instance.ExpMultiplier}";
        }
        else if (buffType == FieldBuffType.DoubleCoin)
        {
            // DoubleCoin은 배율 대신 추가 드롭 방식이므로 고정 텍스트
            multiplierText.text = "+코인";
        }
    }

    System.Collections.IEnumerator PopAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 bigScale = originalScale * 2.5f;
        if (popSound != null) SoundManager.instance.PlaySoundWith(popSound, 1.4f, false, 0);

        float t = 0f;
        float popDuration = 0.1f;

        while (t < popDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, bigScale, t / popDuration);
            yield return null;
        }

        t = 0f;
        while (t < popDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(bigScale, originalScale, t / popDuration);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}