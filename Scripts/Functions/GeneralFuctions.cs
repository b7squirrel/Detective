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

    public Vector2 GetRandomPosInCircle(float _radius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        float distance = Random.Range(0f, _radius);
        float x = distance * Mathf.Cos(angle);
        float y = distance * Mathf.Sin(angle);

        return new Vector2(x, y);
    }

    public Vector2 GetRandomPointInRing(Vector2 center, float outerRadius, float innerRadius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        float radius = Random.Range(innerRadius, outerRadius);

        float x = center.x + radius * Mathf.Cos(angle);
        float y = center.y + radius * Mathf.Sin(angle);

        return new Vector2(x, y);
    }

    public string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes}분 {seconds}초";
    }
}
