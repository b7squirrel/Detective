using UnityEngine;

public class BubbleIndicator : MonoBehaviour
{
    [SerializeField] Transform indicatorCenter;
    [SerializeField] Transform bubble;
    [SerializeField] SpriteRenderer iconSR;

    void Update()
    {
        bubble.position = indicatorCenter.position;

        // �θ� ������Ʈ�ʹ� �ݴ�� ȸ���ؼ� ȸ���� ������ ���� �ʴ´�
        //Quaternion inverseRotation = Quaternion.Inverse(indicatorCenter.localRotation);
        //bubble.localRotation = inverseRotation;

        bubble.eulerAngles = new Vector3 (0, 0, 0);
    }

    public void SetIconImage(Sprite _iconSprite)
    {
        Debug.Log("��������Ʈ �̸� = " +  _iconSprite.name);
        iconSR.sprite = _iconSprite;
    }
}