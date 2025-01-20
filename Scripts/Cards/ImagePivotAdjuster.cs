using UnityEngine;
using UnityEngine.UI;

public class ImagePivotAdjuster : MonoBehaviour
{
    public static ImagePivotAdjuster instance;
    void Awake()
    {
        instance = this;
    }
    public void AdjustImagePosition(Image _image)
    {
        RectTransform rectTransform = _image.rectTransform;
        rectTransform.anchoredPosition = Vector2.zero;
        Sprite sprite = _image.sprite;

        // 스프라이트의 피봇 포인트 추출
        Vector2 spritePivot = Vector2.zero;

        // 스프라이트의 pivot 위치를 0-1 범위의 상대적 위치로 변환
        spritePivot = new Vector2(
            sprite.pivot.x / sprite.rect.width,
            sprite.pivot.y / sprite.rect.height
        );

        // 현재 RectTransform의 피봇과 스프라이트 피봇의 차이 계산
        Vector2 pivotDifference = spritePivot - rectTransform.pivot;

        // 이미지의 크기
        Vector2 size = new Vector2(
            rectTransform.rect.width,
            rectTransform.rect.height
        );

        // 위치 조정값 계산
        Vector2 positionOffset = new Vector2(
            pivotDifference.x * size.x,
            pivotDifference.y * size.y
        );

        // 최종 위치 적용
        rectTransform.anchoredPosition -= positionOffset;
        _image.SetNativeSize();
    }
}