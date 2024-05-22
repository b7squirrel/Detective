using UnityEngine;

public class FieldItemSpawner : MonoBehaviour
{
    [SerializeField] int numPoints;
    [SerializeField] float circleRadius;
    [SerializeField] GameObject[] objectsToSpawn;
    [SerializeField] float frequency;
    [SerializeField] float coinBoxPercentage;
    float nextSpawnTime;
    WallManager wallManager;

    private void Start()
    {
        nextSpawnTime = Time.time + frequency;
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
        if(wallManager == null) wallManager = FindObjectOfType<WallManager>();
        float spawnConst = wallManager.GetSpawnAreaConstant();
        float offset = 2f;
        Vector2 spawnArea = 
            new Vector2(Random.Range(-spawnConst + offset, spawnConst - offset), 
                        Random.Range(-spawnConst + offset, spawnConst - offset));
        return spawnArea;
    }

    void InitSpawnPoints(int _numPoints, float _radius)
    {
        numPoints = _numPoints;
        circleRadius = _radius;
    }
}