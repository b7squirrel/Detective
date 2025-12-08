using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawLineUI : MonoBehaviour
{
    public Image linePrefab;
    Dictionary<string, RectTransform> outPoints = new Dictionary<string, RectTransform>();
    Dictionary<string, RectTransform> inPoints = new Dictionary<string, RectTransform>();
    List<Image> lineImages = new();
    
    // Canvas 참조 추가
    private Canvas parentCanvas;
    
    void Awake()
    {
        // 부모 Canvas 찾기
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("DrawLineUI: Parent Canvas not found!");
        }
    }
    
    void OnEnable()
    {
        foreach (var outPoint in outPoints)
        {
            // inPoints에 같은 키가 있는지 확인
            if (inPoints.ContainsKey(outPoint.Key))
            {
                DrawUILine(outPoint.Value, inPoints[outPoint.Key]);
            }
        }
    }
    
    void OnDisable()
    {
        foreach (Image lineImage in lineImages)
        {
            if (lineImage != null)
            {
                Destroy(lineImage.gameObject); // GameObject를 파괴
            }
        }
        lineImages.Clear(); // 리스트 비우기
    }
    
    public void AddInpoint(string synergyName, RectTransform outPoint)
    {
        outPoints.Add(synergyName, outPoint);
    }
    
    public void AddOutPoint(string synergyName, RectTransform inPoint)
    {
        inPoints.Add(synergyName, inPoint);
    }
    
    public void DrawUILine(RectTransform from, RectTransform to)
    {
        if (from == null || to == null || parentCanvas == null) return;
        
        Image lineImage = Instantiate(linePrefab, transform);
        lineImages.Add(lineImage);
        
        // 월드 좌표를 Canvas의 로컬 좌표로 변환
        Vector3 startWorld = from.position;
        Vector3 endWorld = to.position;
        
        Vector2 startLocal;
        Vector2 endLocal;
        
        // RectTransformUtility를 사용하여 정확한 좌표 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform, 
            RectTransformUtility.WorldToScreenPoint(parentCanvas.worldCamera ?? Camera.main, startWorld), 
            parentCanvas.worldCamera, 
            out startLocal);
            
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform, 
            RectTransformUtility.WorldToScreenPoint(parentCanvas.worldCamera ?? Camera.main, endWorld), 
            parentCanvas.worldCamera, 
            out endLocal);
        
        Vector2 direction = endLocal - startLocal;
        float distance = direction.magnitude;

        // 디버그
        Image endPoint = Instantiate(lineImage, endLocal, Quaternion.identity);
        Image startPoint = Instantiate(lineImage, startLocal, Quaternion.identity);
        endPoint.name = "END POINT";
        startPoint.name = "START POINT";
        endPoint.transform.SetParent(this.transform);
        startPoint.transform.SetParent(this.transform);
        Debug.Log($"start = {startLocal}, end = {endLocal}, distance = {distance}");
        
        // 선 이미지 위치와 크기 설정
        lineImage.rectTransform.anchoredPosition = startLocal + direction * 0.5f;
        lineImage.rectTransform.sizeDelta = new Vector2(distance, 4f); // 4f = 선 두께
        
        // 회전 각도 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineImage.rectTransform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Anchor를 중앙으로 설정
        lineImage.rectTransform.anchorMin = Vector2.one * 0.5f;
        lineImage.rectTransform.anchorMax = Vector2.one * 0.5f;
        lineImage.rectTransform.pivot = Vector2.one * 0.5f;
    }
}