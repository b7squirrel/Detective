using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 애니메이션 이벤트로 레이저 이미지 교체
/// </summary>
public class LaserSpritePicker : MonoBehaviour
{
    List<Sprite> laserSprites;
    SpriteRenderer sr;

    // 애니메이션 이벤트로 스프라이트 교체
    public void SetSprite(int _index)
    {
        if(sr == null) sr = GetComponent<SpriteRenderer>();
        sr.sprite = laserSprites[_index];
    }

    public void SetImages(List<Sprite> _sprites)
    {
        if (laserSprites == null) laserSprites = new List<Sprite>();
        laserSprites = _sprites;
    }
}