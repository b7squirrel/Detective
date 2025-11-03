using UnityEngine;

public class DamageIndicator : MonoBehaviour
{
    public void Init(float radius, Vector2 pos)
    {
        transform.position = pos;
        transform.localScale = radius * Vector2.one; // 인디케이터의 반지름이 1 unit으로 되어 있기 때문에 그냥 radius만 곱해줌
    }
    public void DeactivateIndicator()
    {
        gameObject.SetActive(false);
    }
}
