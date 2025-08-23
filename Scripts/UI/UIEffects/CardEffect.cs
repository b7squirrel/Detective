using UnityEngine;

public class CardEffect : MonoBehaviour
{
    [SerializeField] GameObject cardEffectPrefab; // 카드를 클릭하면 이펙트
    [SerializeField] Transform effectParent; // Present Slot Pool을 할당
    GameObject cardEffect; // 카드 이펙트를 담아두기. 화면에 여러 개가 생기지 않음
    Animator anim;
    public void SetEffectPosition(RectTransform cardRect)
    {
        if (cardEffect == null) cardEffect = Instantiate(cardEffectPrefab, transform);

        RectTransform effectRect = cardEffect.GetComponent<RectTransform>();

        Vector3 cardWorldPos = cardRect.position;
        Vector3 localPos = effectParent.InverseTransformPoint(cardWorldPos);
        effectRect.localPosition = localPos + new Vector3(0f, 24f * cardRect.localScale.y, 0f);

        effectRect.localScale = .5f * cardRect.localScale; // 카드의 크기에 맞게 이펙트의 크기를 조정
        // effectRect.localScale = .3f * Vector2.one;

        if (anim == null) anim = cardEffect.GetComponent<Animator>();
        anim.SetTrigger("Init");
    }
}
