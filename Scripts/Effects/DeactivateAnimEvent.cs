using UnityEngine;

/// <summary>
/// 애니메이션 이벤트에서 자신을 비활성화 하기 위한 클래스
/// </summary>
public class DeactivateAnimEvent : MonoBehaviour
{
    public void DeactivateObject()
    {
        gameObject.SetActive(false);
    }
}
