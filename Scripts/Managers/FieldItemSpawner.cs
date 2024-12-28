using System.Collections;
using UnityEngine;

/// <summary>
/// Game Manager¿¡¼­ Ä³½Ì
/// </summary>
public class FieldItemSpawner : MonoBehaviour
{
    [Header("Item Box")]
    [SerializeField] int numPoints;
    [SerializeField] GameObject objectsToSpawn;
    [SerializeField] float frequency;
    [SerializeField] int maxFieldItemNum;
    float itemBoxSpawnCounter;

    WallManager wallManager;

    [Header("MSB / Multiple Spawn Box")]
    [SerializeField] int numPointsMSB;
    [SerializeField] float frequencyMSB;
    GameObject MSBToSpawn;
    float MSBspawnCounter;

    [Header("Special Box")]
    [SerializeField] GameObject[] gemPrefabs;
    float[] timeIntervals = { 120f, 180f, 240f, 280f, 320f, 380f };

    [Header("Egg Box")]
    [SerializeField] GameObject EggBoxPrefab;
    float[] eggSpawnTime;
    float eggSpawnCoolDown;
    int eggSpawnIndex;

    public void Init(float[] _eggSpawnTime)
    {
        eggSpawnTime = _eggSpawnTime;
    }

    void Start()
    {
        itemBoxSpawnCounter = 0;
        MSBspawnCounter = 0;
    }
    void Update()
    {
        if (itemBoxSpawnCounter >= frequency)
        {
            SpawnObject(objectsToSpawn, numPoints);
            itemBoxSpawnCounter = 0f;
        }
        
        if (MSBspawnCounter >= frequencyMSB)
        {
            int index = Mathf.Clamp((int)(MSBspawnCounter / 60f), 0, timeIntervals.Length - 1);
            MSBToSpawn = gemPrefabs[index];

            SpawnObject(MSBToSpawn, numPointsMSB);
            frequencyMSB += timeIntervals[index];
        }

        itemBoxSpawnCounter += Time.deltaTime;
        MSBspawnCounter += Time.deltaTime;

        if (eggSpawnIndex > eggSpawnTime.Length - 1) return;
        eggSpawnCoolDown += Time.deltaTime;
        if (eggSpawnCoolDown > eggSpawnTime[eggSpawnIndex])
        {
            SpawnEggBox();
            eggSpawnIndex++;
        }
    }
    void SpawnObject(GameObject toSpawn, int _numbersToSpawn)
    {
        for (int i = 0; i < _numbersToSpawn; i++)
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
            for (int i = 0; i < 10; i++)
            {
                SpawnManager.instance.SpawnObject(_position, _toSpawn, _isGem, _exp);
                numberOfItems--;
                if (numberOfItems < 0) break;
            }
            yield return null;
        }
        yield break;
    }
}