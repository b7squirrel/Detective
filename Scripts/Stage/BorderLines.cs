using UnityEngine;

public class BorderLines : MonoBehaviour
{
    [SerializeField] Transform [] rectangleTr; // ���簢���� 4���� ��
    [SerializeField] Transform[] walls; // ��
    LineRenderer lineRenderer;
    Vector2[] rectanglePos = new Vector2[4];

    public void Init()
    {
        // LineRenderer ����
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 5; // ���簢���� 4���� �� + ���������� ���ƿ��� ������ ��

        rectanglePos[0] = new Vector2(walls[0].position.x, walls[3].position.y);
        rectanglePos[1] = new Vector2(walls[0].position.x, walls[2].position.y);
        rectanglePos[2] = new Vector2(walls[1].position.x, walls[2].position.y);
        rectanglePos[3] = new Vector2(walls[1].position.x, walls[3].position.y);

        DrawRectangle();
    }

    void DrawRectangle()
    {
        // 4���� ���� �����ϰ� LineRenderer�� ����
        lineRenderer.SetPosition(0, rectanglePos[0]); // ù ��° ��
        lineRenderer.SetPosition(1, rectanglePos[1]); // �� ��° ��
        lineRenderer.SetPosition(2, rectanglePos[2]); // �� ��° ��
        lineRenderer.SetPosition(3, rectanglePos[3]); // �� ��° ��
        lineRenderer.SetPosition(4, rectanglePos[0]); // ������ ���� ù ��° ������ ���ƿ�
    }
}