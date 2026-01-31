using UnityEngine;
using System.Collections;

public class LaserPointer : MonoBehaviour
{
    // 데이터
    CatDuckWeapon weapon;
    int damage;
    float knockBackChance;
    float knockBackSpeedFactor;
    bool isCriticalDamage;
    string weaponName;
    float sizeOfArea;
    int numberOfCats; // 도착할 고양이 수
    
    [Header("Visual Settings")]
    [SerializeField] SpriteRenderer pointerSprite;
    [SerializeField] float blinkSpeed = 2f; // 깜빡임 속도
    
    [Header("Laser Line Settings")]
    [SerializeField] LineRenderer laserLine;
    [SerializeField] Color laserColor = new Color(1f, 0f, 0f, 0.3f); // 빨간색 반투명
    [SerializeField] float laserWidth = 0.05f; // 레이저 두께
    
    [Header("Cat Spawn Settings")]
    [SerializeField] float spawnDelay = 0.3f; // 레이저 포인터 생성 후 고양이 스폰까지 딜레이
    
    [Header("Fight Cloud Settings")]
    [SerializeField] GameObject fightCloudPrefab; // 싸움 구름 프리펩
    [SerializeField] float fightCloudDuration = 3f; // 싸움 구름 지속 시간
    
    int arrivedCats = 0; // 도착한 고양이 수
    bool allCatsArrived = false;
    Transform player;

    void Awake()
    {
        if (pointerSprite == null)
        {
            pointerSprite = GetComponentInChildren<SpriteRenderer>();
        }
        
        // LineRenderer 설정
        if (laserLine == null)
        {
            laserLine = GetComponent<LineRenderer>();
        }
        
        if (laserLine != null)
        {
            // LineRenderer 초기 설정
            laserLine.startWidth = laserWidth;
            laserLine.endWidth = laserWidth;
            laserLine.startColor = laserColor;
            laserLine.endColor = laserColor;
            laserLine.positionCount = 2; // 시작점과 끝점
            laserLine.useWorldSpace = true; // 월드 좌표 사용
            
            // Material 설정 (기본 Sprites/Default 사용)
            if (laserLine.material == null)
            {
                laserLine.material = new Material(Shader.Find("Sprites/Default"));
            }
        }
        
        // 플레이어 찾기
        player = Player.instance?.transform;
    }

    void OnEnable()
    {
        arrivedCats = 0;
        allCatsArrived = false;
        
        // 플레이어 재확인
        if (player == null)
        {
            player = Player.instance?.transform;
        }
        
        // 레이저 선 활성화
        if (laserLine != null)
        {
            laserLine.enabled = true;
        }
        
        // 깜빡임 시작
        if (pointerSprite != null)
        {
            StartCoroutine(BlinkCo());
        }
    }

    void Update()
    {
        // 레이저 선 업데이트 (고양이들이 모두 도착하기 전까지)
        if (!allCatsArrived && laserLine != null && player != null)
        {
            UpdateLaserLine();
        }
    }

    void UpdateLaserLine()
    {
        // 시작점: 플레이어 위치
        laserLine.SetPosition(0, player.position);
        
        // 끝점: 레이저 포인터 위치
        laserLine.SetPosition(1, transform.position);
    }

    public void Initialize(CatDuckWeapon weapon, int damage, float knockBackChance, float knockBackSpeedFactor, bool isCriticalDamage, string weaponName, float sizeOfArea, int numberOfCats)
    {
        this.weapon = weapon;
        this.damage = damage;
        this.knockBackChance = knockBackChance;
        this.knockBackSpeedFactor = knockBackSpeedFactor;
        this.isCriticalDamage = isCriticalDamage;
        this.weaponName = weaponName;
        this.sizeOfArea = sizeOfArea;
        this.numberOfCats = numberOfCats;
        
        Debug.Log($"LaserPointer: Initialized, waiting for {numberOfCats} cats");
        
        // 고양이 스폰 (약간의 딜레이 후)
        Invoke(nameof(SpawnCats), spawnDelay);
    }

    void SpawnCats()
    {
        if (weapon != null)
        {
            weapon.SpawnCats(transform.position, this, numberOfCats);
        }
    }

    IEnumerator BlinkCo()
    {
        while (!allCatsArrived)
        {
            // 알파값을 0.3 ~ 1.0 사이로 조절
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, 0.7f) + 0.3f;
            
            if (pointerSprite != null)
            {
                Color color = pointerSprite.color;
                color.a = alpha;
                pointerSprite.color = color;
            }
            
            yield return null;
        }
        
        // 깜빡임 종료 - 완전히 보이게
        if (pointerSprite != null)
        {
            Color color = pointerSprite.color;
            color.a = 1f;
            pointerSprite.color = color;
        }
    }

    public void OnCatArrived()
    {
        arrivedCats++;
        Debug.Log($"LaserPointer: Cat arrived! ({arrivedCats}/{numberOfCats})");
        
        // 모든 고양이가 도착했는지 체크
        if (arrivedCats >= numberOfCats && !allCatsArrived)
        {
            allCatsArrived = true;
            OnAllCatsArrived();
        }
    }

    void OnAllCatsArrived()
    {
        Debug.Log("LaserPointer: All cats arrived! Creating fight cloud...");
        
        // 레이저 선 비활성화
        if (laserLine != null)
        {
            laserLine.enabled = false;
        }
        
        // 싸움 구름 생성
        CreateFightCloud();
        
        // 레이저 포인터 비활성화
        Invoke(nameof(Deactivate), 0.2f);
    }

    void CreateFightCloud()
    {
        if (fightCloudPrefab == null)
        {
            Debug.LogError("LaserPointer: Fight cloud prefab not assigned!");
            return;
        }
        
        // 싸움 구름 생성
        GameObject cloudObj = GameManager.instance.poolManager.GetMisc(fightCloudPrefab);
        
        if (cloudObj != null)
        {
            cloudObj.transform.position = transform.position;
            
            // 싸움 구름 초기화
            CatFightCloud fightCloud = cloudObj.GetComponent<CatFightCloud>();
            if (fightCloud != null)
            {
                fightCloud.Initialize(damage, knockBackChance, knockBackSpeedFactor, isCriticalDamage, weaponName, sizeOfArea, fightCloudDuration);
            }
            else
            {
                Debug.LogError("LaserPointer: CatFightCloud component not found!");
            }
        }
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        // Invoke 정리
        CancelInvoke();
        
        // 코루틴 정리
        StopAllCoroutines();
        
        // 레이저 선 비활성화
        if (laserLine != null)
        {
            laserLine.enabled = false;
        }
        
        // 상태 리셋
        arrivedCats = 0;
        allCatsArrived = false;
    }

    // Gizmos로 범위 확인
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sizeOfArea);
    }
}