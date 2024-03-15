using UnityEngine;

public class BubbleIndicator : MonoBehaviour
{
    [SerializeField] Transform indicatorCenter;
    [SerializeField] Transform bubble;
    [SerializeField] SpriteRenderer iconSR;

    void Update()
    {
        bubble.position = indicatorCenter.position;

        // 부모 오브젝트와는 반대로 회전해서 회전에 영향을 받지 않는다
        //Quaternion inverseRotation = Quaternion.Inverse(indicatorCenter.localRotation);
        //bubble.localRotation = inverseRotation;

        bubble.eulerAngles = new Vector3 (0, 0, 0);
    }

    public void SetIconImage(Sprite _iconSprite)
    {
        Debug.Log("스프라이트 이름 = " +  _iconSprite.name);
        iconSR.sprite = _iconSprite;
    }
}