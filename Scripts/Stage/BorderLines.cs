using UnityEngine;

public class BorderLines : MonoBehaviour
{
    [SerializeField] Transform [] rectangleTr; // ���簢���� 4���� ��
    [SerializeField] Transform[] walls; // ��
    LineRenderer lineRenderer;
    LineRenderer[] lineRenderers;
    Vector2[] rectanglePos = new Vector2[4];

    public void Init()
    {
        // LineRenderer ����
        if(lineRenderers == null ) lineRenderers = GetComponentsInChildren<LineRenderer>();

        rectanglePos[0] = new Vector2(walls[0].position.x, walls[3].position.y);
        rectanglePos[1] = new Vector2(walls[0].position.x, walls[2].position.y);
        rectanglePos[2] = new Vector2(walls[1].position.x, walls[2].position.y);
        rectanglePos[3] = new Vector2(walls[1].position.x, walls[3].position.y);

        DrawRectangle();
    }

    void DrawRectangle()
    {
        // 4���� ���� �����ϰ� LineRenderer�� ����
        lineRenderers[0].SetPosition(0, rectanglePos[0]);
        lineRenderers[0].SetPosition(1, rectanglePos[1]);
        lineRenderers[1].SetPosition(0, rectanglePos[1]);
        lineRenderers[1].SetPosition(1, rectanglePos[2]);
        lineRenderers[2].SetPosition(0, rectanglePos[2]);
        lineRenderers[2].SetPosition(1, rectanglePos[3]);
        lineRenderers[3].SetPosition(0, rectanglePos[3]);
        lineRenderers[3].SetPosition(1, rectanglePos[0]);
    }
}