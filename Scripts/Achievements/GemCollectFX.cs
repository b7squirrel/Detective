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
    public GameObject gemPrefab;

    [Header("설정값")]
    public float spreadRadius = 200f;

    [Header("참조")]
    [SerializeField] PlayerDataManager playerDataManager;
    DisplayCurrency displayCurrency; // ui업데이트를 한 번 더 해주기 위해. 누락될 수 있으므로

    [Header("사운드")]
    [SerializeField] AudioClip clipGemSpread;
    [SerializeField] AudioClip clipGemHit;

    bool hasPlayedCollectSound = false;

    public void PlayGemCollectFX(RectTransform pos, int gemAmount)
    {
        hasPlayedCollectSound = false;
        SoundManager.instance.Play(clipGemSpread);

        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, pos.position);

        for (int i = 0; i < gemAmount; i++)
            SpawnOneGem(screenPos);
    }

    private void SpawnOneGem(Vector3 screenPos)
    {
        Vector2 uiStartPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCamera, out uiStartPos);

        GameObject gemObj = Instantiate(gemPrefab, canvasRect);
        RectTransform gemRT = gemObj.GetComponent<RectTransform>();
        gemRT.anchoredPosition = uiStartPos;

        Vector2 randomOffset = Random.insideUnitCircle * spreadRadius;
        Vector2 spreadPos = uiStartPos + randomOffset;

        gemRT.DOAnchorPos(spreadPos, 0.25f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => FlyToTarget(gemRT));
    }

    private void FlyToTarget(RectTransform gemRT)
    {
        Vector3 worldTargetPos = gemTargetIcon.position;

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
                UpdateUIOnly();

                if (!hasPlayedCollectSound)
                {
                    hasPlayedCollectSound = true;
                    SoundManager.instance.Play(clipGemHit);
                }
            });
    }

    void UpdateUIOnly()
    {
        // 빌드 안정성을 위해 강제 UI 업데이트
        if (displayCurrency == null) displayCurrency = FindObjectOfType<DisplayCurrency>();

        if (displayCurrency != null) displayCurrency.UpdateUI();

        if (gemTargetIcon != null)
        {
            gemTargetIcon.DOKill();
            gemTargetIcon.localScale = Vector3.one;
            gemTargetIcon.DOPunchScale(Vector3.one * 0.3f, 0.2f, 1, 0.5f);
        }
    }
}