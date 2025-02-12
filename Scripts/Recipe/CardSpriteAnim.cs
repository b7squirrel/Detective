using UnityEngine;
using UnityEngine.UI;

public class CardSpriteAnim : MonoBehaviour
{
    CardDisp cardDisp;
    SpriteRow[] equipSprites;
    Image[] images;

    public void Init(WeaponData _wd, Image[] _images)
    {
        images = _images;

        if (equipSprites == null) equipSprites = new SpriteRow[4];
        for (int i = 0; i < 4; i++)
        {
            if (_wd.equipSprites == null) return;
            if (_wd.equipSprites[i] != null && _wd.equipSprites[i].sprites.Length > 0)
            {
                images[i].gameObject.SetActive(true);
                equipSprites[i] = _wd.equipSprites[i];
            }
            else
            {
                images[i].gameObject.SetActive(false);
                equipSprites[i] = null;
            }
        }

    }
    public void SetEquippedItemSprite(int _index)
    {
        if (equipSprites == null) return;

        for (int i = 0; i < 4; i++)
        {
            if (equipSprites[i] == null) return;
            if (equipSprites[i].sprites.Length == 1)
            {
                images[i].sprite = equipSprites[i].sprites[0];
            }
            else
            {
                images[i].sprite = equipSprites[i].sprites[_index];
            }
        }
    }
}