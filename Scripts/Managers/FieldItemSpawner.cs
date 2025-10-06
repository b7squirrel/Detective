using System.Collections;
using UnityEngine;

/// <summary>
/// Game Manager에서 캐싱
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
    int eggBoxNums; // 알 상자에 아이디 부여
    int eggNums; // 알에 아이디 부여

    // 알 디버그
    int eggBugNums; // 알의 갯수 오류가 난 횟수

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
        if (itemBoxSpawnCounter >= frequency && GameManager.instance.IsBossStage == false)
        {
            // 보스가 등장한 후에는 스폰이 되지 않도록
            SpawnObject(objectsToSpawn, numPoints);
            itemBoxSpawnCounter = 0f;
        }

        if (MSBspawnCounter >= frequencyMSB && GameManager.instance.IsBossStage == false)
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
        if (eggSpawnCoolDown > eggSpawnTime[eggSpawnIndex] && GameManager.instance.IsBossStage == false)
        {
            SpawnEggBox(GetRandomSpawnPoint());
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
    public void SpawnEggBox(Vector2 spawnPos)
    {
        Transform eggBox = GameManager.instance.poolManager.GetMisc(EggBoxPrefab).transform;
        if (eggBox != null)
        {
            eggBox.position = spawnPos;
            eggBoxNums++; // 알 상자가 스폰될 때 알 상자 갯수 증가
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

    public bool isEggSpawnable()
    {
        eggNums++;
        bool spawnable = eggNums > eggBoxNums ? false : true;
        Debug.Log($"eggNums = {eggNums} , eggBoxNums = {eggBoxNums} - spawnable = {spawnable}");
        if (spawnable == false)
        {
            eggNums--; //알 갯수는 다시 되돌림. 그렇지 않으면 앞으로는 계속 eggNum이 eggBox보다 크게 된다
            eggBugNums++;
        }

        // Debug.LogError($"알 스폰 오류 횟수 = {eggBugNums}");
        return spawnable;
    }
}