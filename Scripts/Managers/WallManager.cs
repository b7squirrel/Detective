using System.Collections;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    // left, right, up, down 순서
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
    
    // StageTime의 Start에서 호출
    public void SetStageDuration(float _duration)
    {
        for (int i = 0; i < starts.Length; i++)
        {
            starts[i].parent = null;
            ends[i].parent = null;

            starts[i].position = startPositions[i];
            ends[i].position = endPositions[i];

            walls[i].position = starts[i].position;

            // 벽을 따라 선을 그림
            GetComponentInChildren<BorderLines>().Init();
        }

        duration = _duration;

        if (isMovingFromStart)
        {
            StartCoroutine(MoveWalls());
        }
    }
    
    /// <summary>
    /// 원래는 보스가 등장할 때 StageEventManager에서 실행할 예정이었으나 
    /// 너무 좁아지면 카메라 바운드가 제대로 작동하지 않아서 보류
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