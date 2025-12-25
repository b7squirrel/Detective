using System.Collections;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    // left, right, up, down ����
    [SerializeField] Transform[] walls;
    [SerializeField] Transform[] starts;
    [SerializeField] Transform[] ends;

    [Header("Wall Position in order of Left, right, Up, Down")]
    [SerializeField] Vector2[] startPositions = new Vector2[4];
    [SerializeField] Vector2[] endPositions = new Vector2[4];

    [SerializeField] bool isMovingFromStart;
    float duration;
    float elapsedTime;
    bool isGameOver;
    
    // StageTime�� Start���� ȣ��
    public void SetStageDuration(float _duration)
    {
        duration = _duration;

        // if (isMovingFromStart)
        // {
        //     StartCoroutine(MoveWalls());
        // }
    }
    public void SetWallSize()
    {
        Vector2[] defaultStartPos =
        {
        new Vector2(-25f, 0f),
        new Vector2(25f, 0f),
        new Vector2(0f, 25f),
        new Vector2(0f, -25f)
    };

        SetWallSize(defaultStartPos);
    }
    public void SetWallSize(Vector2[] startPos)
    {
        for (int i = 0; i < starts.Length; i++)
        {
            starts[i].parent = null;
            ends[i].parent = null;

            starts[i].position = startPos[i];
            ends[i].position = endPositions[i];

            walls[i].position = starts[i].position;

            GetComponentInChildren<BorderLines>().Init();
        }
    }
    
    
    /// <summary>
    /// ������ ������ ������ �� StageEventManager���� ������ �����̾����� 
    /// �ʹ� �������� ī�޶� �ٿ�尡 ����� �۵����� �ʾƼ� ����
    /// </summary>
    public void ActivateMovingWalls()
    {
        StartCoroutine(MoveWalls());
    }

    IEnumerator MoveWalls()
    {
        while (isGameOver == false)
        {
            for (int i = 0; i < walls.Length; i++)
            {
                walls[i].position = Vector2.Lerp(starts[i].position, ends[i].position, elapsedTime / duration);
            }

            elapsedTime += Time.deltaTime;
            if (elapsedTime > duration)
            {
                FindObjectOfType<CharacterGameOver>().GameOver();
                isGameOver = true;
            }

            yield return null;
        }
    }
    public float GetSpawnAreaConstant()
    {
        return walls[1].position.x;
    }
}