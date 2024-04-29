using UnityEngine;
using UnityEngine.UI;

public class SetCostumeImage : MonoBehaviour
{
    Costume costume;
    [SerializeField] Image costumeImage;
    //debug
    [SerializeField] int index;

    public void SetCostumeData(Costume _costume)
    {
        costume = _costume;
        costumeImage.color = new Color(1, 1, 1, 1);
    }
    // 애니메이션 이벤트
    public void SetCostumeSprite(int _index)
    {
        if (costume == null) return;
        costumeImage.sprite = costume.sprites[_index];
        index = _index;
    }
}