using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothScreenPositionController : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] bool useScreenBounds = true;
    [SerializeField] float zDistance = 10f;
    [SerializeField] float moveSpeed = 500f; // ��ũ�� ��ǥ ���� �ʴ� �̵� �ȼ�
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

        // ���� ��ġ�� �ʱ� ��ǥ ��ġ�� ����
        //Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
        //targetScreenPosition = new Vector2(screenPos.x, screenPos.y);
    }

    void Update()
    {
        if (!isMoving) return;

        // ���� ��ũ�� ��ǥ ���ϱ�
        Vector3 currentScreenPos = mainCamera.WorldToScreenPoint(transform.position);
        Vector2 currentPos2D = new Vector2(currentScreenPos.x, currentScreenPos.y);

        // ��ǥ ���������� ����� �Ÿ� ���
        Vector2 direction = (targetScreenPosition - currentPos2D).normalized;
        float distance = Vector2.Distance(currentPos2D, targetScreenPosition);

        // �̵��� �Ÿ� ��� (�����Ӵ�)
        float moveAmount = moveSpeed * Time.deltaTime;

        // ��ǥ ������ ���� �����ߴ��� Ȯ��
        if (distance < moveAmount)
        {
            SetPositionDirectly(targetScreenPosition);
            isMoving = false;
            return;
        }

        // ���ο� ��ũ�� ��ǥ ���
        Vector2 newScreenPos = currentPos2D + direction * moveAmount;

        // ȭ�� ��� ó��
        if (useScreenBounds)
        {
            newScreenPos.x = Mathf.Clamp(newScreenPos.x, 0f, screenWidth);
            newScreenPos.y = Mathf.Clamp(newScreenPos.y, 0f, screenHeight);
        }

        // ��ġ ������Ʈ
        SetPositionDirectly(newScreenPos);
    }

    // ��ǥ ��ũ�� ��ǥ ����
    public void MoveToScreenPosition(Vector2 screenPos)
    {
        targetScreenPosition = screenPos;
        isMoving = true;
    }

    // ��� ��ġ ���� (���� ���)
    private void SetPositionDirectly(Vector2 screenPos)
    {
        Vector3 pos = new Vector3(screenPos.x, screenPos.y, zDistance);
        transform.position = mainCamera.ScreenToWorldPoint(pos);
    }

    // ���� �̵� ������ Ȯ��
    public bool IsMoving()
    {
        return isMoving;
    }

    // �̵� �ӵ� ����
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    // ���� ��ũ�� ��ǥ ��ȯ
    public Vector2 GetCurrentScreenPosition()
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
        return new Vector2(screenPos.x, screenPos.y);
    }
}
