using UnityEngine;

// 적의 레이저 프로젝타일 클래스
public class EnemyLaserProjectile : MonoBehaviour
{
    [SerializeField] LineRenderer laserLine;
    [SerializeField] GameObject hitEffectPrefab;
    [SerializeField] GameObject muzzleFlashPrefab;
    [SerializeField] float maxDistance = 50f;
    [SerializeField] AudioClip[] laserSoundLoops; // 레이져 루프되는 사운드
    [SerializeField] AudioClip[] laserSounds; // 레이져 단발성 사운드
    float laserWidth = .05f; // 레이저 두께
    GameObject hitEffect;
    GameObject muzzleFlash;
    ParticleSystem particleSys;

    private int damage;
    private LayerMask destructables;
    private LayerMask walls;
    private int frameCount = 5; // 데미지 처리 간격
    bool isSoundPaused; // 사운드가 일시 정지 되었다면 timeScale이 0이 아닐 때 다시 재생하기 위해

    void Start()
    {
        // LineRenderer 설정
        // if (laserLine == null)
        // {
        //     laserLine = GetComponent<LineRenderer>();
        //     if (laserLine == null)
        //     {
        //         laserLine = gameObject.AddComponent<LineRenderer>();
        //     }
        // }

        // LineRenderer 기본 설정
        laserLine.material = new Material(Shader.Find("Sprites/Default"));
        // laserLine.color = Color.red;
        laserLine.startWidth = laserWidth;  // 시작점 두께
        laserLine.endWidth = laserWidth;    // 끝점 두께
        laserLine.positionCount = 2;
        laserLine.sortingLayerName = "Effect"; // Sorting Layer를 Effect로 설정
        laserLine.sortingOrder = 1;

        // 히트 이펙트 설정
        if (hitEffect == null)
        {
            hitEffect = Instantiate(hitEffectPrefab, transform);
            particleSys = GetComponentInChildren<ParticleSystem>();
        }

        // 머즐 플래시 설정
        if (muzzleFlash == null)
        {
            muzzleFlash = Instantiate(muzzleFlashPrefab, transform);
            muzzleFlash.transform.localScale = .4f * Vector2.one;
        }
    }

    void OnEnable()
    {
        if (laserLine != null)
        {
            laserLine.enabled = true;
        }
    }

    void OnDisable()
    {
        if (laserLine != null)
        {
            laserLine.enabled = false;
        }
        if (hitEffect != null)
        {
            hitEffect.SetActive(false);
        }
        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(false);
        }
        //사운드 정지. 오디오클립이나 사운드매니져가 널이라면 건너뜀
        if (laserSoundLoops == null || laserSoundLoops.Length == 0)
            return;

        var sm = SoundManager.instance;
        if (sm == null)
        {
            Debug.LogWarning("SoundManager.instance가 null입니다. 사운드 중단을 건너뜁니다.");
            return;
        }
        foreach (var item in laserSoundLoops)
        {
            SoundManager.instance.StopLoop(item);
        }
    }

    public void Initialize(int _damage, LayerMask _destructables, LayerMask _walls, float _laserWidth = 1f)
    {
        damage = _damage;
        destructables = _destructables;
        walls = _walls;
        laserWidth = _laserWidth;

        // LineRenderer 두께 업데이트
        if (laserLine != null)
        {
            laserLine.startWidth = laserWidth;
            laserLine.endWidth = laserWidth;
        }

        foreach (var item in laserSoundLoops)
        {
            SoundManager.instance.PlayLoop(item);
        }
        foreach (var item in laserSounds)
        {
            SoundManager.instance.Play(item);
        }
    }

    void Update()
    {
        // 타임스케일이 0이 되었을 때 — 사운드 정지 (한 번만)
    if (Time.timeScale == 0 && !isSoundPaused)
    {
        foreach (var item in laserSoundLoops)
        {
            SoundManager.instance.StopLoop(item);
        }
        isSoundPaused = true; // 일시정지 상태로 표시
    }

    // 타임스케일이 다시 1로 돌아왔을 때 — 사운드 재개 (한 번만)
    else if (Time.timeScale > 0 && isSoundPaused)
    {
        foreach (var item in laserSoundLoops)
        {
            SoundManager.instance.PlayLoop(item);
        }
        isSoundPaused = false;
    }

    // 플레이어가 죽으면 즉시 전기 사운드 중단
    if (GameManager.instance.IsPlayerDead)
    {
        foreach (var item in laserSoundLoops)
        {
            SoundManager.instance.StopLoop(item);
        }
        isSoundPaused = false;
        return; // 이후 처리 생략
    }

    CastLaser();
    }

    void CastLaser()
    {
        Vector2 startPos = transform.position;
        Vector2 direction = transform.right; // 로컬 X축 방향
        Vector2 endPos = startPos + direction * maxDistance;

        // 모든 레이어를 검사하여 가장 가까운 충돌점 찾기
        LayerMask allLayers = destructables | walls;
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, maxDistance, allLayers);

        Vector2 laserEndPoint = hit.collider != null ? hit.point : endPos;

        // 레이저 라인 그리기
        laserLine.SetPosition(0, startPos);
        laserLine.SetPosition(1, laserEndPoint);

        // 히트 이펙트 위치 설정
        if (hitEffect != null)
        {
            hitEffect.transform.position = laserEndPoint;
            hitEffect.SetActive(hit.collider != null);

            if (Time.frameCount % 30 == 0) particleSys.Play(); ;// 30프레임 간격으로 파티클 재생
        }

        // 머즐 플래시 위치 설정
        if (muzzleFlash != null)
        {
            muzzleFlash.transform.position = startPos;
            muzzleFlash.SetActive(true);
        }

        // 데미지 처리 (일정 간격으로) - 두꺼운 레이저를 위해 여러 레이캐스트 사용
            if (Time.frameCount % frameCount == 0)
            {
                // 레이저 중심선과 양쪽 가장자리에서 레이캐스트 실행
                Vector2 perpendicular = new Vector2(-direction.y, direction.x); // 수직 벡터
                float halfWidth = laserWidth / 2f;

                // 중심, 위쪽, 아래쪽에서 레이캐스트
                Vector2[] rayPositions = {
                startPos, // 중심
                startPos + perpendicular * halfWidth, // 위쪽
                startPos - perpendicular * halfWidth, // 아래쪽
                startPos + perpendicular * halfWidth * 0.5f, // 중간 위
                startPos - perpendicular * halfWidth * 0.5f  // 중간 아래
            };

                foreach (Vector2 rayPos in rayPositions)
                {
                    RaycastHit2D[] hits = Physics2D.RaycastAll(rayPos, direction, maxDistance, destructables);
                    foreach (var hitTarget in hits)
                    {
                        if (hit.collider == null) return;

                        // 그냥 getcomponent와는 다르게 메모리 할당 없음
                        if (hit.collider.TryGetComponent<Character>(out Character character))
                        {
                            character.TakeDamage(damage, EnemyType.Melee, SlimeAttackType.Electricity);
                        }
                    }
                }
            }
    }
}