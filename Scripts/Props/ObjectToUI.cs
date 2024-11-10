using UnityEngine;

public class ObjectToUI : MonoBehaviour
{
    [SerializeField] Canvas targetCanvas; // Screen Space - Overlay ĵ����
    [SerializeField] RectTransform uiPrefab;

    public RectTransform SpawnUIAtWorldPosition(Vector3 worldPosition)
    {
        // 1. ���� ��ǥ�� ��ũ�� ��ǥ�� ��ȯ
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);

        // 2. ��ũ�� ��ǥ�� ĵ������ ���� ��ǥ�� ��ȯ
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetCanvas.GetComponent<RectTransform>(),
            screenPoint,
            null, // Screen Space - Overlay������ ī�޶� null�� ����
            out localPoint);

        // 3. UI ������Ʈ ���� �� ��ġ ����
        RectTransform spawnedUI = Instantiate(uiPrefab, targetCanvas.transform);
        spawnedUI.localPosition = localPoint;

        return spawnedUI;
    }
}
