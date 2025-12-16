using UnityEngine;
using DG.Tweening;

/// <summary>
/// 보석 UI
/// 실제 보석의 수는 이 클래스를 호출하기 전에 더해지고 저장되어야 함
/// </summary>
public class GemCollectFX : MonoBehaviour
{
    [Header("필요 요소")]
    public RectTransform canvasRect;
    public Camera uiCamera;
    public RectTransform gemTargetIcon;
    public RectTransform coinTargetIcon;
    public GameObject gemPrefab;
    public GameObject coinPrefab;

    [Header("설정값")]
    public float spreadRadius = 200f;

    [Header("참조")]
    DisplayCurrency displayCurrency; // ui업데이트를 한 번 더 해주기 위해. 누락될 수 있으므로

    [Header("사운드")]
    [SerializeField] AudioClip clipGemSpread;
    [SerializeField] AudioClip clipGemHit;
    [SerializeField] AudioClip clipCoinSpread;
    [SerializeField] AudioClip clipCoinHit;

    bool hasPlayedCollectSound = false;

    public void PlayGemCollectFX(RectTransform pos, int gemAmount, bool isGem)
    {
        hasPlayedCollectSound = false;
        SoundManager.instance.Play(isGem ? clipGemSpread : clipCoinSpread);

        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, pos.position);

        if (gemAmount > 30) gemAmount = 30;

        for (int i = 0; i < gemAmount; i++)
            SpawnOneGem(screenPos, isGem);
    }

    private void SpawnOneGem(Vector3 screenPos, bool isGem)
    {
        Vector2 uiStartPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCamera, out uiStartPos);

        GameObject gemObj = Instantiate(isGem? gemPrefab : coinPrefab, canvasRect);
        RectTransform gemRT = gemObj.GetComponent<RectTransform>();
        gemRT.anchoredPosition = uiStartPos;

        Vector2 randomOffset = Random.insideUnitCircle * spreadRadius;
        Vector2 spreadPos = uiStartPos + randomOffset;

        gemRT.DOAnchorPos(spreadPos, 0.25f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => FlyToTarget(gemRT, isGem));
    }

    private void FlyToTarget(RectTransform gemRT, bool isGem)
    {
        RectTransform targetPoint = isGem ? gemTargetIcon : coinTargetIcon;
        Vector3 worldTargetPos = targetPoint.position;

        Vector2 localTargetPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gemRT.parent as RectTransform,
            RectTransformUtility.WorldToScreenPoint(uiCamera, worldTargetPos),
            uiCamera,
            out localTargetPos);

        float offset = Random.Range(-0.1f, 0.1f);

        gemRT.DOAnchorPos(localTargetPos, 0.35f + offset)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                Destroy(gemRT.gameObject);

                // UI만 업데이트 (실제 값은 이미 추가됨)
                UpdateUIOnly(isGem);

                if (!hasPlayedCollectSound)
                {
                    hasPlayedCollectSound = true;
                    SoundManager.instance.Play(isGem ? clipGemHit : clipCoinHit);
                }
            });
    }

    void UpdateUIOnly(bool isGem)
    {
        RectTransform targetPoint = isGem ? gemTargetIcon : coinTargetIcon;

        // 빌드 안정성을 위해 강제 UI 업데이트
        if (displayCurrency == null) displayCurrency = FindObjectOfType<DisplayCurrency>();

        if (displayCurrency != null)
        {
            // 숫자 텍스트 애니메이션 추가
            displayCurrency.UpdateUI();
            displayCurrency.AnimateTextChange(isGem); // ← 새로 추가할 메서드
        }

        // 아이콘 펀치 애니메이션
        if (targetPoint != null)
        {
            targetPoint.DOKill();
            targetPoint.localScale = Vector3.one;
            targetPoint.DOPunchScale(Vector3.one * 0.3f, 0.2f, 1, 0.5f)
            .OnComplete(() =>
            {
                targetPoint.localScale = Vector3.one;
            });
        }
    }
}