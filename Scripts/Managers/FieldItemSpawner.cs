using System.Collections;
using UnityEngine;

public class FieldItemSpawner : MonoBehaviour
{
    [Header("Item Box")]
    [SerializeField] int numPoints;
    [SerializeField] GameObject objectsToSpawn;
    [SerializeField] float frequency;
    float nextSpawnTime;
    WallManager wallManager;

    int sortingLayerID;

    [Header("Egg Box")]
    [SerializeField] GameObject EggBoxPrefab;
    float[] eggSpawnTime;
    float eggSpawnCoolDown;
    int eggSpawnIndex;

    void Start()
    {
        nextSpawnTime = Time.time + frequency;
        eggSpawnTime = FindObjectOfType<StageEvenetManager>().GetEggSpawnTimes();
    }
    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnObject(objectsToSpawn);
            nextSpawnTime = Time.time + frequency;
        }

        if (eggSpawnIndex > eggSpawnTime.Length - 1) return;
        eggSpawnCoolDown += Time.deltaTime;
        if (eggSpawnCoolDown > eggSpawnTime[eggSpawnIndex])
        {
            SpawnEggBox();
            eggSpawnIndex++;
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
    public void SpawnEggBox()
    {
        Transform eggBox = GameManager.instance.poolManager.GetMisc(EggBoxPrefab).transform;
        if (eggBox != null)
        {
            eggBox.position = GetRandomSpawnPoint();
        }
    }
    Vector2 GetRandomSpawnPoint()
    {
        if (wallManager == null) wallManager = FindObjectOfType<WallManager>();
        float spawnConst = wallManager.GetSpawnAreaConstant();
        float offset = 2f;
        Vector2 spawnArea =
            new Vector2(Random.Range(-spawnConst + offset, spawnConst - offset),
                        Random.Range(-spawnConst + offset, spawnConst - offset));
        return spawnArea;
    }

    public void SpawnMultipleObjects(int _nums, GameObject _toSpawn, Vector2 _position, int _exp)
    {
        StartCoroutine(GenItems(_nums, _toSpawn, _position, _exp));
    }
    IEnumerator GenItems(int _nums, GameObject _toSpawn, Vector2 _position, int _exp)
    {
        int numberOfItems = _nums;
        Debug.Log("Number Of Items to drop = " + numberOfItems);
        bool _isGem;
        if (_toSpawn.GetComponent<GemPickUpObject>() != null)
        {
            _isGem = true;
        }
        else
        {
            _isGem = false;
        }

        while (numberOfItems > 0)
        {
            SpawnManager.instance.SpawnObject(_position, _toSpawn, _isGem, _exp);
            numberOfItems--;
            yield return null;
        }
        yield break;
    }
}