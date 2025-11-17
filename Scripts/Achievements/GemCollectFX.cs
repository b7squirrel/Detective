using UnityEngine;
using TMPro;
using DG.Tweening;
using Unity.VisualScripting;

public class GemCollectFX : MonoBehaviour
{
    [Header("필요 요소")]
    public RectTransform canvasRect;        // Canvas RectTransform
    public Camera uiCamera;                 // Canvas Render Camera
    public RectTransform gemTargetIcon;     // 목표 보석 아이콘
    public GameObject gemPrefab;            // 작은 보석 프리팹

    [Header("설정값")]
    public float spreadRadius = 200f;       // 퍼지는 범위

    [Header("참조")]
    [SerializeField] PlayerDataManager playerDataManager;

    [Header("사운드")]
    [SerializeField] AudioClip clipGemSpread;
    [SerializeField] AudioClip clipGemHit;
    bool hasPlayedCollectSound = false;

    /// <summary>
    /// 보석 FX 실행
    /// </summary>
    public void PlayGemCollectFX(RectTransform pos, int gemAmount)
    {
        hasPlayedCollectSound = false; 
        SoundManager.instance.Play(clipGemSpread);

        // 1) Canvas 내 UI RectTransform 위치 → 스크린 포지션
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, pos.position);
        
        for (int i = 0; i < gemAmount; i++)
        {
            SpawnOneGem(screenPos);
        }
    }

    private void SpawnOneGem(Vector3 screenPos)
    {
        // Canvas 좌표로 변환
        Vector2 uiStartPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            uiCamera,
            out uiStartPos
        );

        // 보석 생성 (어떤 부모든 상관없음)
        GameObject gemObj = Instantiate(gemPrefab, canvasRect);
        RectTransform gemRT = gemObj.GetComponent<RectTransform>();
        gemRT.anchoredPosition = uiStartPos;

        // 랜덤 퍼짐
        Vector2 randomOffset = Random.insideUnitCircle * spreadRadius;
        Vector2 spreadPos = uiStartPos + randomOffset;

        // 1) 퍼짐
        gemRT.DOAnchorPos(spreadPos, 0.25f).SetEase(Ease.OutQuad)
            .OnComplete(() => FlyToTarget(gemRT));
    }

    private void FlyToTarget(RectTransform gemRT)
    {
        // 목표 아이콘의 월드 위치
        Vector3 worldTargetPos = gemTargetIcon.position;

        // gemRT 부모 기준 로컬 좌표로 변환
        Vector2 localTargetPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gemRT.parent as RectTransform,
            RectTransformUtility.WorldToScreenPoint(uiCamera, worldTargetPos),
            uiCamera,
            out localTargetPos
        );

        // 2) 목표로 날아가기
        float offset = UnityEngine.Random.Range(-.1f, .1f);
        gemRT.DOAnchorPos(localTargetPos, 0.35f + offset).SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                Destroy(gemRT.gameObject);
                AddGem(1);

                if (hasPlayedCollectSound == false)
                {
                    hasPlayedCollectSound = true;
                    SoundManager.instance.Play(clipGemHit);
                }
            });
    }

    private void AddGem(int amount)
    {
        int currentValue = playerDataManager.GetCurrentHighCoinNumber();

        // 숫자 증가 (null-safe)
        playerDataManager.SetCristalNumberAs(currentValue + 1);

        // 아이콘 팝 효과
        if (gemTargetIcon != null)
        {
            gemTargetIcon.DOKill(); // 기존 Tween 제거
            gemTargetIcon.localScale = Vector3.one; // 스케일 초기화
            gemTargetIcon.DOPunchScale(Vector3.one * 0.3f, 0.2f, 1, 0.5f);
        }
    }
}