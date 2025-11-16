using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GemCollectFX : MonoBehaviour
{
    [Header("필요 요소")]
    public RectTransform canvasRect;        // Canvas RectTransform
    public Camera uiCamera;                 // Canvas Render Camera
    public RectTransform gemTargetIcon;     // 목표 보석 아이콘
    public GameObject gemPrefab;            // 작은 보석 프리팹
    public TextMeshProUGUI gemCountText;    // 보석 개수 표시 텍스트

    [Header("설정값")]
    public int currentGem = 0;              // 현재 보석 개수
    public float spreadRadius = 200f;       // 퍼지는 범위
    public int spawnCount = 10;             // 생성할 보석 수

    [Header("Debug")]
    public RectTransform testPos;

    /// <summary>
    /// 보석 FX 실행
    /// </summary>
    public void PlayGemCollectFX(RectTransform pos)
    {
        // 1) Canvas 내 UI RectTransform 위치 → 스크린 포지션
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, pos.position);
        
        for (int i = 0; i < spawnCount; i++)
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
        gemRT.DOAnchorPos(localTargetPos, 0.35f).SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                Destroy(gemRT.gameObject);
                AddGem(1);
            });
    }

    private void AddGem(int amount)
    {
        int startValue = currentGem;
        int endValue = currentGem + amount;
        currentGem = endValue;

        // 3) 숫자 증가
        DOTween.To(() => startValue, x =>
        {
            gemCountText.text = x.ToString();
        }, endValue, 0.3f);

        // 4) 아이콘 팝 효과
        gemTargetIcon.DOPunchScale(Vector3.one * 0.3f, 0.2f, 1, 0.5f);
    }
}