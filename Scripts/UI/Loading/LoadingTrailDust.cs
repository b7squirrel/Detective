using UnityEngine;

public class LoadingTrailDust : MonoBehaviour
{
    [Header("먼지 설정")]
    [SerializeField] GameObject dustPrefab; // 먼지 프리팹 (Animator 포함)
    [SerializeField] float spawnInterval = 0.1f; // 먼지 생성 간격 (초)
    [SerializeField] float dustLifetime; // 먼지가 사라지는 시간을 애니메이션 타임라인의 초+프레임 입력. 실제로는 60으로 나누어서 초로 바꿔서 사용
    [SerializeField] Transform myParent; 
    bool activateDust; // 애니메이션 이벤트에서 제어

    float spawnTimer;
    RectTransform rectTransform;
    Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = FindObjectOfType<Canvas>();
        spawnTimer = 0f;
    }

    void Update()
    {
        if (activateDust == false) return;

        // 타이머 업데이트
        spawnTimer += Time.deltaTime;

        // 먼지 생성
        if (spawnTimer >= spawnInterval)
        {
            SpawnDust();
            spawnTimer = 0f;
        }
    }

    void SpawnDust()
    {
        // 먼지 오브젝트 생성
        GameObject dust = Instantiate(dustPrefab, myParent);

        // 현재 위치에 먼지 배치
        RectTransform dustRect = dust.GetComponent<RectTransform>();
        Vector2 offset = new Vector2(Random.Range(-.6f, .6f), Random.Range(-.2f, .2f));
        dustRect.position = (Vector2)rectTransform.position + offset;
        dustRect.localScale = rectTransform.localScale;

        // 일정 시간 후 먼지 제거
        Destroy(dust, dustLifetime / 60f);
    }

    // 애니메이션 이벤트
    public void SetDustActivate(int activate)
    {
        bool dustActivate = activate == 1 ? true : false;
        activateDust = dustActivate;
    }
}
