using UnityEngine;

public class WallManager : MonoBehaviour
{
    // left, right, up, down ¼ø¼­
    [SerializeField] Transform[] walls;
    [SerializeField] Transform[] starts;
    [SerializeField] Transform[] ends;
    [SerializeField] float duration = 300f; // 5ºÐ
    float elapsedTime;
    bool isGameOver;

    void Update()
    {
        if (isGameOver) return;

        for (int i = 0; i < walls.Length; i++)
        {
            walls[i].position = Vector2.Lerp(starts[i].position, ends[i].position, elapsedTime / duration);
        }
        
        elapsedTime += Time.deltaTime;
        if (elapsedTime > duration)
        {
            Debug.Log("Game Over");
            isGameOver = true;
        }
            
    }
}