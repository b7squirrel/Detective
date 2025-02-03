using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode] // 에디터에서도 스크립트가 실행되도록 함
public class ImageUtility : MonoBehaviour
{
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
    }

    public void SetImageNativeSize(Image _image)
    {
        if (_image == null) return;
        _image.SetNativeSize();
    }

    // 에디터에서 값이 변경될 때마다 호출됨
    void OnValidate()
    {
        // 현재 게임오브젝트의 Image 컴포넌트를 가져와서 적용
        Image image = GetComponent<Image>();
        if (image != null)
        {
            SetImageNativeSize(image);
            AdjustImagePosition(image);
            Debug.Log("변경된 사항을 적용했습니다.");
        }
    }
}
