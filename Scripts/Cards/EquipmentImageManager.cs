using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class EquipmentImageManager : MonoBehaviour
{
    [SerializeField] private Image[] equipmentImages; // Inspector에서 머리, 가슴, 얼굴, 손 순서대로 할당

    [Header("디버깅")]
    [SerializeField] Sprite[] equipmentSprites;

    // private void OnValidate()
    // {
    //     if (equipmentImages == null) return;

    //     foreach (Image image in equipmentImages)
    //     {
    //         if (image != null && image.sprite != null)
    //             AdjustImagePosition(image);
    //         Debug.Log($"{image.name}가 교체되었습니다.");
    //     }
    // }

    [ContextMenu("Update Images")]
    public void UpdateImages()
    {
        if (equipmentImages == null) return;

        for (int i = 0; i < equipmentImages.Length; i++)
        {
            if (equipmentSprites[i] == null) continue;
            equipmentImages[i].sprite = equipmentSprites[i];
            Debug.Log($"{equipmentImages[i].name}의 스프라이트는 {equipmentImages[i].sprite.name}입니다.");
        }

        foreach (Image image in equipmentImages)
        {
            if (image != null && image.sprite != null)
                AdjustImagePosition(image);
            Debug.Log($"{image.name}가 교체되었습니다.");
        }
    }

    /// <summary>
    /// 인덱스에 해당하는 장비 이미지의 스프라이트를 교체하고 위치를 조정합니다.
    /// </summary>
    /// <param name="index">교체할 이미지의 인덱스</param>
    /// <param name="newSprite">새 스프라이트</param>
    public void ReplaceEquipment(int index, Sprite newSprite)
    {
        if (index >= 0 && index < equipmentImages.Length && newSprite != null)
        {
            equipmentImages[index].sprite = newSprite;
            // 스프라이트 교체 후 위치를 바로 조정
            AdjustImagePosition(equipmentImages[index]);
        }
    }

    /// <summary>
    /// 이미지의 RectTransform과 스프라이트의 피벗 차이를 계산하여 위치를 조정합니다.
    /// </summary>
    /// <param name="image">조정할 Image 컴포넌트</param>
    private void AdjustImagePosition(Image image)
    {
        if (image == null || image.sprite == null)
            return;

        RectTransform rectTransform = image.rectTransform;
        // 초기화
        rectTransform.anchoredPosition = Vector2.zero;
        Sprite sprite = image.sprite;

        // 스프라이트의 피벗 위치 (정규화)
        Vector2 spritePivot = new Vector2(
            sprite.pivot.x / sprite.rect.width,
            sprite.pivot.y / sprite.rect.height
        );

        // RectTransform과 스프라이트 피벗 간의 차이
        Vector2 pivotDifference = spritePivot - rectTransform.pivot;
        Vector2 size = rectTransform.rect.size;

        // 위치 오프셋 계산
        // Vector2 positionOffset = new Vector2(
        //     pivotDifference.x * size.x,
        //     pivotDifference.y * size.y
        // );
        Vector2 positionOffset = new Vector2(
                pivotDifference.x * sprite.rect.width,
                pivotDifference.y * sprite.rect.height
        );
                

        rectTransform.anchoredPosition -= positionOffset;
        image.SetNativeSize();
    }
    public Image GetEquipmentImage(int _index)
    {
        return equipmentImages[_index];
    }
}
