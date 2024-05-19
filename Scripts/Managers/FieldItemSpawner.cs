using UnityEngine;

public class FieldItemSpawner : MonoBehaviour
{
    [SerializeField] int numPoints;
    [SerializeField] float circleRadius;
    [SerializeField] GameObject[] objectsToSpawn;
    [SerializeField] float frequency;
    [SerializeField] float coinBoxPercentage;
    float nextSpawnTime;
    Player player;

    private void Start()
    {
        nextSpawnTime = Time.time + frequency;
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }
    }
    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            int index = Random.Range(0, 100f) > coinBoxPercentage ? 0 : 1;
            SpawnObject(objectsToSpawn[index]);
            nextSpawnTime = Time.time + frequency;
        }
    }
    void SpawnObject(GameObject toSpawn)
    {
        for (int i = 0; i < numPoints; i++)
        {
            Transform pickUP = GameManager.instance.poolManager.GetMisc(toSpawn).transform;
            if (pickUP != null)
            {
                pickUP.position = GetRandomSpawnPoint();
            }
        }
    }
    Vector2 GetRandomSpawnPoint()
    {
        // 랜덤한 반지름과 각도 생성
        float r = circleRadius * Mathf.Sqrt(Random.value);
        float theta = Random.value * 2 * Mathf.PI;

        // 극좌표를 직교좌표로 변환
        float x = r * Mathf.Cos(theta);
        float y = r * Mathf.Sin(theta);

        return new Vector2(x, y) + (Vector2)player.transform.position;
    }

    void InitSpawnPoints(int _numPoints, float _radius)
    {
        numPoints = _numPoints;
        circleRadius = _radius;
    }
}