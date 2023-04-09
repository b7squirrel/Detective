using UnityEngine;

public class GeneralFuctions
{
    public Vector2 GetRandomPositionFrom(Vector2 targetPos ,float rangeX, float rangeY)
    {
        float f = UnityEngine.Random.value > .5f ? -1f : 1f;
        float randomX = 0f;
        float randomY = 0f;
        if (UnityEngine.Random.value > .5f)
        {
            randomX = UnityEngine.Random.Range(targetPos.x - rangeX, targetPos.x + rangeX);
            randomY = targetPos.y + (f * 10f);
        }
        else
        {
            randomY = UnityEngine.Random.Range(targetPos.y - rangeY, targetPos.y + rangeY);
            randomX = targetPos.x + (f * 3f);
        }
        return new Vector2(randomX, randomY);
    }
}
