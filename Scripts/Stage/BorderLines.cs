using UnityEngine;

public class BorderLines : MonoBehaviour
{
    [SerializeField] Transform [] rectangleTr; // 직사각형의 4개의 점
    [SerializeField] Transform[] walls; // 벽
    LineRenderer lineRenderer;
    Vector2[] rectanglePos = new Vector2[4];

    public void Init()
    {
        // LineRenderer 설정
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 5; // 직사각형은 4개의 점 + 시작점으로 돌아오는 마지막 점

        rectanglePos[0] = new Vector2(walls[0].position.x, walls[3].position.y);
        rectanglePos[1] = new Vector2(walls[0].position.x, walls[2].position.y);
        rectanglePos[2] = new Vector2(walls[1].position.x, walls[2].position.y);
        rectanglePos[3] = new Vector2(walls[1].position.x, walls[3].position.y);

        DrawRectangle();
    }

    void DrawRectangle()
    {
        // 4개의 점을 지정하고 LineRenderer에 전달
        lineRenderer.SetPosition(0, rectanglePos[0]); // 첫 번째 점
        lineRenderer.SetPosition(1, rectanglePos[1]); // 두 번째 점
        lineRenderer.SetPosition(2, rectanglePos[2]); // 세 번째 점
        lineRenderer.SetPosition(3, rectanglePos[3]); // 네 번째 점
        lineRenderer.SetPosition(4, rectanglePos[0]); // 마지막 점은 첫 번째 점으로 돌아옴
    }
}