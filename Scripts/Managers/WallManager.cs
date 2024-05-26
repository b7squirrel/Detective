using UnityEngine;

public class WallManager : MonoBehaviour
{
    // left, right, up, down ¼ø¼­
    [SerializeField] Transform[] walls;
    [SerializeField] Transform[] starts;
    [SerializeField] Transform[] ends;
    [SerializeField] float duration;
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
            FindObjectOfType<CharacterGameOver>().GameOver();
            isGameOver = true;
        }
    }
    public float GetSpawnAreaConstant()
    {
        return walls[1].position.x;
    }
    public void SetStageDuration(float _duration)
    {
        duration = _duration;
    }
}