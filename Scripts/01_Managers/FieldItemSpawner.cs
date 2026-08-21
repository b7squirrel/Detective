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
    [SerializeField] int maxEggBoxCount = 5; // ⭐ 이 값은 이제 Start()에서 동료 여유분에 맞춰 매 스테이지 재계산됨 (인스펙터 값은 폴백용 기본치)
    float[] eggSpawnTime = { 10f, 50f, 90f, 60f, 230f };
    float eggSpawnCoolDown;
    int eggSpawnIndex;
    int eggBoxNums; // 알 상자에 아이디 부여
    int eggNums; // 알에 아이디 부여

    GameConfig gameConfig;

    // 알 디버그
    int eggBugNums; // 알의 갯수 오류가 난 횟수

    void Start()
    {
        itemBoxSpawnCounter = 0;
        MSBspawnCounter = 0;
        gameConfig = Resources.Load<GameConfig>("GameConfig");

        // ⭐ 추가: 로비에서 데려온 동료 수를 감안해서, 이번 스테이지에서 알 상자가 몇 개까지 스폰될 수 있는지 계산
        InitMaxEggBoxCountForThisStage();
    }

    // ⭐ 추가: 동료 최대 인원(WeaponManager.MaxCompanions) - 로비에서 데려온 동료 수 = 이번 스테이지에서 필드로 얻을 수 있는 여유분
    // 일반 스테이지는 eggSpawnTime 배열 길이(5)로도 별도 제한되므로, 이 값이 5보다 크더라도 실제로는 5개를 넘지 않음
    void InitMaxEggBoxCountForThisStage()
    {
        int initialCompanionCount = 0;
        if (GameManager.instance != null && GameManager.instance.startingDataContainer != null)
        {
            var companions = GameManager.instance.startingDataContainer.GetCompanions();
            initialCompanionCount = companions != null ? companions.Count : 0;
        }

        int maxCompanionsAllowed = maxEggBoxCount; // WeaponManager를 못 찾았을 때의 폴백 (인스펙터 기본값 유지)
        WeaponManager weaponManager = Player.instance != null ? Player.instance.GetComponent<WeaponManager>() : null;
        if (weaponManager != null)
        {
            maxCompanionsAllowed = weaponManager.MaxCompanions;
        }
        else
        {
            Logger.LogWarning("[FieldItemSpawner] WeaponManager를 찾지 못해 알 상자 최대 개수를 기본값으로 둡니다.");
        }

        maxEggBoxCount = Mathf.Max(0, maxCompanionsAllowed - initialCompanionCount);
        Logger.Log($"[FieldItemSpawner] 스쿼드 동료 {initialCompanionCount}마리 → 이번 스테이지 알 상자 최대 {maxEggBoxCount}개");
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

        if (gameConfig != null && gameConfig.hidePeriodicChest) return;

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
        if (eggBoxNums >= maxEggBoxCount) return;
        
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
                PickupSpawner.Instance.SpawnPickup(_position, _toSpawn, _isGem, _exp);
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
        // Logger.LogError($"알 스폰 오류 횟수 = {eggBugNums}");
        return spawnable;
    }
}