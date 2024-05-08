using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ִϸ��̼� �̺�Ʈ�� ������ �̹��� ��ü
/// </summary>
public class LaserSpritePicker : MonoBehaviour
{
    List<Sprite> laserSprites;
    SpriteRenderer sr;

    // �ִϸ��̼� �̺�Ʈ�� ��������Ʈ ��ü
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