using System;
using UnityEngine;
using UnityEngine.UI;

public class CardSpriteAnim : MonoBehaviour
{
    [SerializeField] SpriteRow[] equipSprites;
    Image[] images; // 장비를 표시할 이미지칸 배열

    public void Init(Image[] _images)
    {
        equipSprites = new SpriteRow[4]; // 이미지가 남지 않도록 초기화해서 참조가 남지 않게 함
        if(images == null) 
        {
            images = _images;
            
        }
    }
    public void StoreItemSpriteRow(int _index, SpriteRow _spriteRow)
    {
        equipSprites[_index] = _spriteRow; // 해당 index의 애니메이션 이미지를 저장
    }

    // 애니메이션 이벤트로 사용
    public void SetEquippedItemSprite(int _index)
    {
        if (equipSprites == null) return; // equipSprites 자체가 null이면 종료

        for (int i = 0; i < 4; i++)
        {
            if (equipSprites[i] == null) continue; // 개별 equipSprites[i]가 null이면 건너뛰기
            if (equipSprites[i].sprites == null) continue;

            if (equipSprites[i].sprites.Length == 1)
            {
                images[i].sprite = equipSprites[i].sprites[0]; // 이미지가 1개라면 계속 그 첫 번째 이미지만 주입
            }
            else if (_index >= 0 && _index < equipSprites[i].sprites.Length)
            {
                images[i].sprite = equipSprites[i].sprites[_index]; // 넘겨 받은 index의 그림을 주입
                images[i].SetNativeSize();
            }
        }
    }
}