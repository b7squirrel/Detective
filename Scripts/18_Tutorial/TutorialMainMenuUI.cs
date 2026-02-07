using UnityEngine;

public class TutorialMainMenuUI : MonoBehaviour
{
    [SerializeField] GameObject handPrefab;

    public void GenerateHand(Transform parentObj, bool isSR)
    {
        // 왼쪽에서 오른쪽을 가리키는 손인지, 오른쪽에서 왼쪽(SR)으로 가리키는 것인지
        GameObject hand = Instantiate(handPrefab, parentObj);
        float localScaleX = isSR ? -1f : 1f;
        hand.transform.localScale = new Vector3(localScaleX, 1f, 1f);
    }
}
