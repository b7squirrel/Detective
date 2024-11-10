using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothScreenPositionController : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] bool useScreenBounds = true;
    [SerializeField] float zDistance = 10f;
    [SerializeField] float moveSpeed = 500f; // 스크린 좌표 기준 초당 이동 픽셀
    [SerializeField] 

    Vector2 targetScreenPosition;
    bool isMoving = false;
    float screenWidth;
    float screenHeight;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        screenWidth = Screen.width;
        screenHeight = Screen.height;

        // 현재 위치를 초기 목표 위치로 설정
        //Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
        //targetScreenPosition = new Vector2(screenPos.x, screenPos.y);
    }

    void Update()
    {
        if (!isMoving) return;

        // 현재 스크린 좌표 구하기
        Vector3 currentScreenPos = mainCamera.WorldToScreenPoint(transform.position);
        Vector2 currentPos2D = new Vector2(currentScreenPos.x, currentScreenPos.y);

        // 목표 지점까지의 방향과 거리 계산
        Vector2 direction = (targetScreenPosition - currentPos2D).normalized;
        float distance = Vector2.Distance(currentPos2D, targetScreenPosition);

        // 이동할 거리 계산 (프레임당)
        float moveAmount = moveSpeed * Time.deltaTime;

        // 목표 지점에 거의 도달했는지 확인
        if (distance < moveAmount)
        {
            SetPositionDirectly(targetScreenPosition);
            isMoving = false;
            return;
        }

        // 새로운 스크린 좌표 계산
        Vector2 newScreenPos = currentPos2D + direction * moveAmount;

        // 화면 경계 처리
        if (useScreenBounds)
        {
            newScreenPos.x = Mathf.Clamp(newScreenPos.x, 0f, screenWidth);
            newScreenPos.y = Mathf.Clamp(newScreenPos.y, 0f, screenHeight);
        }

        // 위치 업데이트
        SetPositionDirectly(newScreenPos);
    }

    // 목표 스크린 좌표 설정
    public void MoveToScreenPosition(Vector2 screenPos)
    {
        targetScreenPosition = screenPos;
        isMoving = true;
    }

    // 즉시 위치 설정 (내부 사용)
    private void SetPositionDirectly(Vector2 screenPos)
    {
        Vector3 pos = new Vector3(screenPos.x, screenPos.y, zDistance);
        transform.position = mainCamera.ScreenToWorldPoint(pos);
    }

    // 현재 이동 중인지 확인
    public bool IsMoving()
    {
        return isMoving;
    }

    // 이동 속도 설정
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    // 현재 스크린 좌표 반환
    public Vector2 GetCurrentScreenPosition()
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
        return new Vector2(screenPos.x, screenPos.y);
    }
}
