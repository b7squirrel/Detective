using UnityEngine;

public class ObjectToUI : MonoBehaviour
{
    [SerializeField] Canvas targetCanvas; // Screen Space - Overlay 캔버스
    [SerializeField] RectTransform uiPrefab;

    public RectTransform SpawnUIAtWorldPosition(Vector3 worldPosition)
    {
        // 1. 월드 좌표를 스크린 좌표로 변환
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);

        // 2. 스크린 좌표를 캔버스의 로컬 좌표로 변환
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetCanvas.GetComponent<RectTransform>(),
            screenPoint,
            null, // Screen Space - Overlay에서는 카메라를 null로 설정
            out localPoint);

        // 3. UI 오브젝트 생성 및 위치 설정
        RectTransform spawnedUI = Instantiate(uiPrefab, targetCanvas.transform);
        spawnedUI.localPosition = localPoint;

        return spawnedUI;
    }
}
