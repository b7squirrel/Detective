using System.Collections;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    // left, right, up, down ¼ø¼­
    [SerializeField] Transform[] walls;
    [SerializeField] Transform[] starts;
    [SerializeField] Transform[] ends;

    [SerializeField] bool isMovingWall;
    float duration;
    float elapsedTime;
    bool isGameOver;

    public void SetStageDuration(float _duration)
    {
        if (isMovingWall)
        {
            for (int i = 0; i < starts.Length; i++)
            {
                starts[i].parent = null;
                ends[i].parent = null;
            }

            duration = _duration;
            StartCoroutine(MoveWalls());
        }
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