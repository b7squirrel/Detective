using UnityEngine;

public class BorderLines : MonoBehaviour
{
    [SerializeField] Transform [] rectangleTr; // 직사각형의 4개의 점
    [SerializeField] Transform[] walls; // 벽
    LineRenderer lineRenderer;
    LineRenderer[] lineRenderers;
    Vector2[] rectanglePos = new Vector2[4];

    public void Init()
    {
        // LineRenderer 설정
        if(lineRenderers == null ) lineRenderers = GetComponentsInChildren<LineRenderer>();

        rectanglePos[0] = new Vector2(walls[0].position.x, walls[3].position.y);
        rectanglePos[1] = new Vector2(walls[0].position.x, walls[2].position.y);
        rectanglePos[2] = new Vector2(walls[1].position.x, walls[2].position.y);
        rectanglePos[3] = new Vector2(walls[1].position.x, walls[3].position.y);

        DrawRectangle();
    }

    void DrawRectangle()
    {
        // 4개의 점을 지정하고 LineRenderer에 전달
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